using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Parsing;

/// <summary>
/// Парсер mobile.de (Европа) через Playwright
/// </summary>
public partial class MobileDeParser
{
    private readonly IBrowser _browser;
    private readonly ILogger<MobileDeParser> _logger;
    private readonly HashSet<string> _seenUrls = new();

    public MobileDeParser(IBrowser browser, ILogger<MobileDeParser> logger)
    {
        _browser = browser;
        _logger = logger;
    }

    /// <summary>
    /// Распарсить страницу поиска mobile.de
    /// </summary>
    public async Task<MobileDeSearchResult> ParseSearchPageAsync(
        string searchUrl,
        int pageIndex = 1,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mobile.de парсинг: {Url}, страница {Page}", searchUrl, pageIndex);

        try
        {
            var page = await _browser.NewPageAsync();

            try
            {
                // Формируем URL с пагинацией
                var url = pageIndex == 1 
                    ? searchUrl 
                    : $"{searchUrl}&pageNumber={pageIndex}";

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
                    return new MobileDeSearchResult();
                }

                // Ждём появления элементов с автомобилями
                try
                {
                    await page.WaitForSelectorAsync("[data-testid='vehicle-card']", new() { Timeout = 30000 });
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

                return new MobileDeSearchResult
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
            _logger.LogError(ex, "Ошибка при парсинге mobile.de: {Url}", searchUrl);
            throw;
        }
    }

    /// <summary>
    /// Получить все автомобили с пагинацией
    /// </summary>
    public async IAsyncEnumerable<MobileDeCarDto> ParseAllAsync(
        string searchUrl,
        int maxPages = 20,
        int targetCount = 100,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int page = 1;
        int totalParsed = 0;

        _logger.LogInformation("Начало парсинга mobile.de: цель {Target} авто, макс. {MaxPages} страниц", 
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

    private async Task<List<MobileDeCarDto>> ParseCarsAsync(IPage page)
    {
        var cars = new List<MobileDeCarDto>();

        try
        {
            // Находим все карточки автомобилей
            var carCards = await page.QuerySelectorAllAsync("[data-testid='vehicle-card']");

            foreach (var card in carCards)
            {
                try
                {
                    var car = new MobileDeCarDto();

                    // Название
                    var titleElement = await card.QuerySelectorAsync(".js-title-link");
                    car.Title = await titleElement?.TextContentAsync();

                    // Ссылка на объявление
                    var urlElement = await card.QuerySelectorAsync("a.js-title-link");
                    var href = await urlElement?.GetAttributeAsync("href");
                    car.Url = !string.IsNullOrEmpty(href) 
                        ? (href.StartsWith("http") ? href : $"https://www.mobile.de{href}")
                        : null;

                    // Изображение
                    var imgElement = await card.QuerySelectorAsync("img");
                    var src = await imgElement?.GetAttributeAsync("src");
                    car.ImageUrl = src;

                    // Цена
                    var priceElement = await card.QuerySelectorAsync("[data-testid='price-primary']");
                    var priceText = await priceElement?.TextContentAsync();
                    car.Price = ParsePrice(priceText);

                    // Год, пробег, топливо, коробка - в нижней части карточки
                    var details = await card.QuerySelectorAllAsync(".rk-preset");
                    foreach (var detail in details)
                    {
                        var text = await detail.TextContentAsync();
                        if (!string.IsNullOrEmpty(text))
                        {
                            ParseDetail(car, text.Trim());
                        }
                    }

                    // Город и страна
                    var locationElement = await card.QuerySelectorAsync("[data-testid='seller-location']");
                    var locationText = await locationElement?.TextContentAsync();
                    if (!string.IsNullOrEmpty(locationText))
                    {
                        var parts = locationText.Split(',');
                        if (parts.Length >= 1)
                            car.City = parts[0].Trim();
                        if (parts.Length >= 2)
                            car.Country = parts[1].Trim();
                    }

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

    private void ParseDetail(MobileDeCarDto car, string text)
    {
        // Год
        if (Regex.IsMatch(text, @"^\d{4}$"))
        {
            car.Year = text;
            return;
        }

        // Пробег
        var mileageMatch = MileagePattern().Match(text);
        if (mileageMatch.Success)
        {
            car.Mileage = mileageMatch.Value;
            return;
        }

        // Топливо
        if (text.Contains("Diesel") || text.Contains("Benzin") || text.Contains("Hybrid") || 
            text.Contains("Elektro") || text.Contains("LPG"))
        {
            car.Fuel = text;
            return;
        }

        // Коробка
        if (text.Contains("Schaltgetriebe") || text.Contains("Automatik") || 
            text.Contains("Halbautomatik"))
        {
            car.Transmission = text;
            return;
        }

        // Мощность
        var powerMatch = PowerPattern().Match(text);
        if (powerMatch.Success)
        {
            car.Power = powerMatch.Value;
            return;
        }
    }

    private async Task<int> GetTotalPagesAsync(IPage page)
    {
        try
        {
            var paginationElement = await page.QuerySelectorAsync("[data-testid='pagination']");
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

        // Удаляем символы валюты и пробелы
        var cleanPrice = priceText.Replace("€", "").Replace(".", "").Replace(",", ".").Trim();
        
        if (decimal.TryParse(cleanPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
        {
            return price.ToString("F2");
        }

        return priceText;
    }

    [GeneratedRegex(@"\d{1,3}(?:\.\d{3})*\s*km")]
    private static partial Regex MileagePattern();

    [GeneratedRegex(@"\d+\s*kW\s*\(\s*\d+\s*PS\s*\)")]
    private static partial Regex PowerPattern();
}
