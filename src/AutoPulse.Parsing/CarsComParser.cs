using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Parsing;

/// <summary>
/// Парсер cars.com (США) через Playwright
/// </summary>
public partial class CarsComParser
{
    private readonly IBrowser _browser;
    private readonly ILogger<CarsComParser> _logger;
    private readonly HashSet<string> _seenUrls = new();

    public CarsComParser(IBrowser browser, ILogger<CarsComParser> logger)
    {
        _browser = browser;
        _logger = logger;
    }

    /// <summary>
    /// Распарсить страницу поиска cars.com
    /// </summary>
    public async Task<CarsComSearchResult> ParseSearchPageAsync(
        string searchUrl,
        int pageIndex = 1,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cars.com парсинг: {Url}, страница {Page}", searchUrl, pageIndex);

        try
        {
            var page = await _browser.NewPageAsync();

            try
            {
                // Формируем URL с пагинацией
                var url = pageIndex == 1 
                    ? searchUrl 
                    : $"{searchUrl}&page={pageIndex}";

                _logger.LogInformation("Переход на URL: {Url}", url);

                // Переходим на страницу
                var response = await page.GotoAsync(url, new() 
                { 
                    WaitUntil = WaitUntilState.NetworkIdle, 
                    Timeout = 120000 
                });

                if (response != null && response.Status >= 400)
                {
                    _logger.LogError("Ошибка HTTP {Status}: {Url}", response.Status, url);
                    return new CarsComSearchResult();
                }

                // Закрываем cookie баннер если есть
                try
                {
                    var cookieButton = await page.WaitForSelectorAsync("[data-testid='cookie-consent-accept-button']", new() { Timeout = 5000 });
                    if (cookieButton != null)
                    {
                        await cookieButton.ClickAsync();
                        await page.WaitForTimeoutAsync(1000);
                    }
                }
                catch (TimeoutException)
                {
                    // Нет cookie баннера
                }

                // Ждём появления элементов с автомобилями
                try
                {
                    await page.WaitForSelectorAsync("[data-cy='vehicleCard']", new() { Timeout = 30000 });
                }
                catch (TimeoutException)
                {
                    _logger.LogWarning("Не найдены карточки автомобилей. Возможно, страница не загрузилась.");
                }

                // Скроллинг для загрузки контента
                await ScrollPageAsync(page);

                // Парсим автомобили
                var cars = await ParseCarsAsync(page);

                // Получаем информацию о пагинации
                var totalPages = await GetTotalPagesAsync(page);

                _logger.LogInformation("Распаршено {Count} автомобилей, всего страниц: {TotalPages}", 
                    cars.Count, totalPages);

                return new CarsComSearchResult
                {
                    Cars = cars,
                    CurrentPage = pageIndex,
                    TotalPages = totalPages,
                    TotalResults = cars.Count * totalPages
                };
            }
            finally
            {
                await page.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при парсинге cars.com: {Url}", searchUrl);
            throw;
        }
    }

    /// <summary>
    /// Получить все автомобили с пагинацией
    /// </summary>
    public async IAsyncEnumerable<CarsComCarDto> ParseAllAsync(
        string searchUrl,
        int maxPages = 20,
        int targetCount = 100,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int page = 1;
        int totalParsed = 0;

        _logger.LogInformation("Начало парсинга cars.com: цель {Target} авто, макс. {MaxPages} страниц", 
            targetCount, maxPages);

        while (page <= maxPages && totalParsed < targetCount && !cancellationToken.IsCancellationRequested)
        {
            var result = await ParseSearchPageAsync(searchUrl, page, cancellationToken);

            // Фильтруем дубликаты
            var newCars = result.Cars.Where(c => !string.IsNullOrEmpty(c.Url) && !_seenUrls.Contains(c.Url)).ToList();
            foreach (var car in newCars)
            {
                if (!string.IsNullOrEmpty(car.Url))
                    _seenUrls.Add(car.Url);
            }

            foreach (var car in newCars)
            {
                yield return car;
            }

            totalParsed += newCars.Count;
            page++;

            _logger.LogInformation("Страниц: {Page}, всего авто: {Total}", page - 1, totalParsed);

            if (!result.HasNextPage || newCars.Count == 0)
            {
                _logger.LogInformation("Достигнут конец списка");
                yield break;
            }

            // Задержка между запросами
            await Task.Delay(2000, cancellationToken);
        }
    }

    private async Task<List<CarsComCarDto>> ParseCarsAsync(IPage page)
    {
        var cars = new List<CarsComCarDto>();

        try
        {
            // Находим все карточки автомобилей
            var carCards = await page.QuerySelectorAllAsync("[data-cy='vehicleCard']");

            foreach (var card in carCards)
            {
                try
                {
                    var car = new CarsComCarDto();

                    // Название и ссылка
                    var titleElement = await card.QuerySelectorAsync("[data-cy='vehicleTitle'] a");
                    car.Title = await titleElement?.TextContentAsync();
                    var href = await titleElement?.GetAttributeAsync("href");
                    car.Url = !string.IsNullOrEmpty(href) 
                        ? (href.StartsWith("http") ? href : $"https://www.cars.com{href}")
                        : null;

                    // Изображение
                    var imgElement = await card.QuerySelectorAsync("img[src*='vehicle']");
                    var src = await imgElement?.GetAttributeAsync("src");
                    car.ImageUrl = src;

                    // Цена
                    var priceElement = await card.QuerySelectorAsync("[data-cy='primaryPrice']");
                    var priceText = await priceElement?.TextContentAsync();
                    car.Price = ParsePrice(priceText);

                    // Год, модель из заголовка
                    if (!string.IsNullOrEmpty(car.Title))
                    {
                        var (year, brand, model) = ParseTitle(car.Title);
                        car.Year = year;
                        car.Brand = brand;
                        car.Model = model;
                    }

                    // Детали (пробег, топливо, коробка и т.д.)
                    var detailsElement = await card.QuerySelectorAsync("[data-cy='vehicleDetails']");
                    if (detailsElement != null)
                    {
                        var detailsText = await detailsElement.TextContentAsync();
                        ParseDetails(car, detailsText ?? "");
                    }

                    // Локация
                    var locationElement = await card.QuerySelectorAsync("[data-cy='dealerLocation']");
                    var locationText = await locationElement?.TextContentAsync();
                    if (!string.IsNullOrEmpty(locationText))
                    {
                        ParseLocation(car, locationText);
                    }

                    // Дилер
                    var dealerElement = await card.QuerySelectorAsync("[data-cy='dealerName']");
                    car.DealerName = await dealerElement?.TextContentAsync();

                    if (!string.IsNullOrEmpty(car.Title))
                    {
                        cars.Add(car);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка при парсинге карточки автомобиля");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка автомобилей");
        }

        return cars;
    }

    private (string year, string brand, string model) ParseTitle(string title)
    {
        // Пример: "2019 Audi A3 2.0T Premium"
        var parts = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        string year = "";
        string brand = "";
        string model = "";

        if (parts.Length > 0 && int.TryParse(parts[0], out var y) && y >= 1900 && y <= DateTime.UtcNow.Year + 1)
        {
            year = parts[0];
        }

        if (parts.Length > 1)
        {
            brand = parts[1];
        }

        if (parts.Length > 2)
        {
            model = string.Join(" ", parts.Skip(2));
        }

        return (year, brand, model);
    }

    private void ParseDetails(CarsComCarDto car, string details)
    {
        // Пробег
        var mileageMatch = MileagePattern().Match(details);
        if (mileageMatch.Success)
        {
            car.Mileage = mileageMatch.Value.Replace(",", "");
        }

        // Топливо
        if (details.Contains("Diesel") || details.Contains("Gasoline") || details.Contains("Hybrid") || 
            details.Contains("Electric") || details.Contains("Flex Fuel"))
        {
            car.Fuel = details.Contains("Gasoline") ? "Gasoline" :
                       details.Contains("Diesel") ? "Diesel" :
                       details.Contains("Hybrid") ? "Hybrid" :
                       details.Contains("Electric") ? "Electric" : "Flex Fuel";
        }

        // Коробка
        if (details.Contains("Automatic") || details.Contains("Manual") || details.Contains("CVT"))
        {
            car.Transmission = details.Contains("Automatic") ? "Automatic" :
                               details.Contains("Manual") ? "Manual" : "CVT";
        }

        // Привод
        if (details.Contains("AWD") || details.Contains("FWD") || details.Contains("RWD") || details.Contains("4WD"))
        {
            car.Drivetrain = details.Contains("AWD") ? "AWD" :
                             details.Contains("FWD") ? "FWD" :
                             details.Contains("RWD") ? "RWD" : "4WD";
        }

        // Двигатель
        var engineMatch = EnginePattern().Match(details);
        if (engineMatch.Success)
        {
            car.Engine = engineMatch.Value;
        }
    }

    private void ParseLocation(CarsComCarDto car, string locationText)
    {
        // Пример: "Chicago, IL 60601"
        var parts = locationText.Split(',');
        if (parts.Length >= 1)
            car.City = parts[0].Trim();
        if (parts.Length >= 2)
        {
            var stateZip = parts[1].Trim().Split(' ');
            car.State = stateZip.FirstOrDefault() ?? "";
            car.ZipCode = stateZip.Length > 1 ? stateZip[1] : "";
        }
    }

    private async Task<int> GetTotalPagesAsync(IPage page)
    {
        try
        {
            var paginationElement = await page.QuerySelectorAsync("[data-cy='pagination']");
            if (paginationElement != null)
            {
                var pageLinks = await paginationElement.QuerySelectorAllAsync("a");
                if (pageLinks.Any())
                {
                    var maxPage = 1;
                    foreach (var link in pageLinks)
                    {
                        var text = await link.TextContentAsync();
                        if (int.TryParse(text, out var p) && p > maxPage)
                        {
                            maxPage = p;
                        }
                    }
                    return maxPage;
                }
            }

            // Альтернативно: ищем текст "of X pages"
            var pageInfoElement = await page.QuerySelectorAsync("[data-cy='paginationInfo']");
            if (pageInfoElement != null)
            {
                var text = await pageInfoElement.TextContentAsync();
                var match = System.Text.RegularExpressions.Regex.Match(text ?? "", "of\\s+(\\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out var pages))
                {
                    return pages;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка при получении количества страниц");
        }

        return 1;
    }

    private async Task ScrollPageAsync(IPage page)
    {
        for (int i = 0; i < 5; i++)
        {
            await page.WaitForTimeoutAsync(1000);
            await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
            await page.WaitForTimeoutAsync(500);
            await page.EvaluateAsync("window.scrollTo(0, 0)");
        }
    }

    private string? ParsePrice(string? priceText)
    {
        if (string.IsNullOrEmpty(priceText))
            return null;

        // Удаляем символы валюты и запятые
        var cleanPrice = priceText.Replace("$", "").Replace(",", "").Trim();
        
        if (decimal.TryParse(cleanPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
        {
            return price.ToString("F2");
        }

        return priceText;
    }

    [GeneratedRegex(@"\d{1,3}(?:,\d{3})*\s*mi\.?")]
    private static partial Regex MileagePattern();

    [GeneratedRegex(@"\d+\.\d+L\s+\w+")]
    private static partial Regex EnginePattern();
}
