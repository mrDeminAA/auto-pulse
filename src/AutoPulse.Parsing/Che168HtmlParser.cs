using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Parsing;

/// <summary>
/// Парсер Che168 через парсинг HTML страниц
/// Избегает проблемы с _sign в API
/// </summary>
public partial class Che168HtmlParser
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Che168HtmlParser> _logger;

    // Base URL для поиска
    private const string SearchBaseUrl = "https://www.che168.com/beijing/aodi/a3/";

    public Che168HtmlParser(HttpClient httpClient, ILogger<Che168HtmlParser> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Настраиваем заголовки браузера
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
        );
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en;q=0.8");
        _httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.che168.com/");
    }

    /// <summary>
    /// Распарсить страницу поиска и получить список автомобилей
    /// </summary>
    public async Task<IReadOnlyList<ParsedCarData>> ParseSearchPageAsync(
        string brandUrl,
        int pageIndex = 1,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Парсинг HTML страницы: {Url}, страница {Page}", brandUrl, pageIndex);

        try
        {
            // Формируем URL с пагинацией
            var url = pageIndex > 1 ? $"{brandUrl.TrimEnd('/')}o{pageIndex}/" : brandUrl;
            
            _logger.LogDebug("Запрос URL: {Url}", url);

            var html = await _httpClient.GetStringAsync(url, cancellationToken);
            _logger.LogDebug("Получен HTML, размер: {Size} байт", html.Length);

            // Сохраняем HTML для отладки (первую страницу)
            if (pageIndex == 1)
            {
                var debugPath = Path.Combine(Path.GetTempPath(), $"che168-page-{DateTime.Now:yyyyMMdd-HHmmss}.html");
                await File.WriteAllTextAsync(debugPath, html, cancellationToken);
                _logger.LogDebug("HTML сохранен для отладки: {Path}", debugPath);
            }

            var cars = ParseHtml(html);
            _logger.LogInformation("Распаршено {Count} автомобилей со страницы {Page}", cars.Count, pageIndex);

            return cars;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при парсинге HTML страницы {Url}", brandUrl);
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

            _logger.LogInformation("Обработано страниц: {Page}, всего авто: {Total}", page - 1, totalParsed);

            // Задержка между запросами чтобы не забанили
            await Task.Delay(1000, cancellationToken);
        }
    }

    private List<ParsedCarData> ParseHtml(string html)
    {
        var cars = new List<ParsedCarData>();

        // Паттерн 1: Ищем все ссылки на /home/ID
        var linkPattern = CarLinkRegex();
        var matches = linkPattern.Matches(html);

        _logger.LogDebug("Найдено {Count} ссылок на автомобили", matches.Count);

        foreach (Match match in matches)
        {
            try
            {
                var carId = match.Groups["id"].Value;
                var carName = match.Groups["name"].Value;

                if (!string.IsNullOrEmpty(carId) && long.TryParse(carId, out var id))
                {
                    var car = new ParsedCarData
                    {
                        Id = id,
                        FullName = string.IsNullOrEmpty(carName) ? $"Car {id}" : carName.Trim(),
                        SourceUrl = $"https://www.che168.com/home/{carId}",
                        ImageUrl = "https://cdn.pixabay.com/photo/2020/09/06/07/37/car-5548243_1280.jpg",
                        Source = "Che168"
                    };

                    // Парсим название для получения бренда, модели, года
                    var (brand, model, year) = ParseCarName(carName);
                    car.Brand = brand;
                    car.Model = model;
                    car.Year = year;

                    cars.Add(car);
                    _logger.LogDebug("Добавлен автомобиль: {Id} - {Brand} {Model} {Year}", 
                        car.Id, car.Brand, car.Model, car.Year);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка при парсинге автомобиля из: {Match}", match.Value);
            }
        }

        return cars;
    }

    private (string brand, string model, int year) ParseCarName(string carName)
    {
        // Пример: "奥迪 A3 2023 款 35 TFSI"
        var parts = carName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        string brand = "";
        string model = "";
        int year = 0;

        if (parts.Length > 0)
        {
            brand = parts[0]; // 奥迪
        }

        for (int i = 1; i < parts.Length; i++)
        {
            var part = parts[i];

            // Ищем год (например, "2023 款")
            if (part.Contains("款"))
            {
                var yearPart = part.Replace("款", "");
                if (int.TryParse(yearPart, out var y) && y >= 1900 && y <= DateTime.UtcNow.Year + 1)
                {
                    year = y;
                    continue;
                }
            }

            // Добавляем к модели
            if (!string.IsNullOrEmpty(part) && part.Length > 1)
            {
                model += part + " ";
            }
        }

        return (brand, model.Trim(), year);
    }

    // Regex для поиска ссылок на автомобили
    // Ищем: /home/1234567 и текст рядом
    [GeneratedRegex(@"<a[^>]*?href=""(?<url>/home/(?<id>\d+))""[^>]*?>(?<name>.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex CarLinkRegex();
}
