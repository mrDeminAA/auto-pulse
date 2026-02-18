using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Parsing;

/// <summary>
/// Парсер Che168 через Playwright (headless браузер)
/// Рендерит JavaScript и получает реальный HTML
/// </summary>
public class Che168PlaywrightParser
{
    private readonly IBrowser _browser;
    private readonly ILogger<Che168PlaywrightParser> _logger;

    public Che168PlaywrightParser(IBrowser browser, ILogger<Che168PlaywrightParser> logger)
    {
        _browser = browser;
        _logger = logger;
    }

    /// <summary>
    /// Распарсить страницу поиска
    /// </summary>
    public async Task<IReadOnlyList<ParsedCarData>> ParseSearchPageAsync(
        string brandUrl,
        int pageIndex = 1,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Playwright парсинг: {Url}, страница {Page}", brandUrl, pageIndex);

        try
        {
            var url = pageIndex > 1 ? $"{brandUrl.TrimEnd('/')}o{pageIndex}/" : brandUrl;
            
            using var page = await _browser.NewPageAsync();
            
            // Переходим на страницу
            await page.GotoAsync(url, new() { WaitUntil = WaitUntilState.NetworkIdle });
            
            // Ждем загрузки карточек автомобилей
            await page.WaitForSelectorAsync("a[href*='/home/']", new() { Timeout = 10000 });
            
            // Получаем HTML после рендеринга JavaScript
            var html = await page.ContentAsync();
            _logger.LogDebug("Получен HTML после рендеринга JS, размер: {Size} байт", html.Length);

            // Сохраняем HTML для отладки
            if (pageIndex == 1)
            {
                var debugPath = Path.Combine(Path.GetTempPath(), $"che168-rendered-{DateTime.Now:yyyyMMdd-HHmmss}.html");
                await File.WriteAllTextAsync(debugPath, html, cancellationToken);
                _logger.LogDebug("HTML сохранен: {Path}", debugPath);
            }

            // Парсим автомобили
            var cars = await ParseCarsFromPageAsync(page);
            _logger.LogInformation("Распаршено {Count} автомобилей", cars.Count);

            return cars;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при парсинге {Url}", brandUrl);
            throw;
        }
    }

    /// <summary>
    /// Получить все автомобили с пагинацией
    /// </summary>
    public async IAsyncEnumerable<ParsedCarData> ParseAllAsync(
        string brandUrl,
        int maxPages = 100,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int page = 1;
        int totalParsed = 0;

        while (page <= maxPages && !cancellationToken.IsCancellationRequested)
        {
            var cars = await ParseSearchPageAsync(brandUrl, page, cancellationToken);

            if (cars.Count == 0)
            {
                _logger.LogInformation("Достигнут конец списка на странице {Page}", page);
                yield break;
            }

            foreach (var car in cars)
            {
                yield return car;
            }

            totalParsed += cars.Count;
            page++;

            _logger.LogInformation("Страниц: {Page}, всего авто: {Total}", page - 1, totalParsed);

            // Задержка между запросами
            await Task.Delay(2000, cancellationToken);
        }
    }

    private async Task<List<ParsedCarData>> ParseCarsFromPageAsync(IPage page)
    {
        var cars = new List<ParsedCarData>();

        // Ищем все ссылки на автомобили
        var carLinks = await page.QuerySelectorAllAsync("a[href*='/home/']");

        _logger.LogDebug("Найдено {Count} ссылок на автомобили", carLinks.Count);

        foreach (var link in carLinks)
        {
            try
            {
                var href = await link.GetAttributeAsync("href");
                var text = await link.TextContentAsync();

                if (!string.IsNullOrEmpty(href) && href.Contains("/home/"))
                {
                    // Извлекаем ID из URL
                    var idPart = href.Split("/home/").LastOrDefault();
                    if (!string.IsNullOrEmpty(idPart) && long.TryParse(idPart, out var id))
                    {
                        // Извлекаем изображение если есть
                        var img = await link.QuerySelectorAsync("img");
                        var imgSrc = img != null ? await img.GetAttributeAsync("src") : null;

                        var car = new ParsedCarData
                        {
                            Id = id,
                            FullName = text?.Trim() ?? $"Car {id}",
                            SourceUrl = $"https://www.che168.com{href}",
                            ImageUrl = NormalizeImageUrl(imgSrc),
                            Source = "Che168"
                        };

                        // Парсим название
                        var (brand, model, year) = ParseCarName(text);
                        car.Brand = brand;
                        car.Model = model;
                        car.Year = year;

                        cars.Add(car);
                        _logger.LogDebug("Добавлен: {Id} - {Brand} {Model}", car.Id, car.Brand, car.Model);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка при парсинге ссылки");
            }
        }

        return cars;
    }

    private string NormalizeImageUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return "https://cdn.pixabay.com/photo/2020/09/06/07/37/car-5548243_1280.jpg";

        if (url.StartsWith("//"))
            return $"https:{url}";

        if (url.StartsWith("http"))
            return url;

        return "https://cdn.pixabay.com/photo/2020/09/06/07/37/car-5548243_1280.jpg";
    }

    private (string brand, string model, int year) ParseCarName(string? carName)
    {
        if (string.IsNullOrEmpty(carName))
            return ("", "", 0);

        var parts = carName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string brand = parts.Length > 0 ? parts[0] : "";
        string model = "";
        int year = 0;

        for (int i = 1; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.Contains("款") && int.TryParse(part.Replace("款", ""), out var y) && y >= 1900 && y <= DateTime.UtcNow.Year + 1)
            {
                year = y;
            }
            else if (part.Length > 1)
            {
                model += part + " ";
            }
        }

        return (brand, model.Trim(), year);
    }
}
