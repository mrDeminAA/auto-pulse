using System.Net.Http;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Application.Parsing;

/// <summary>
/// Парсер для китайского сайта Autohome (www.autohome.com.cn)
/// </summary>
public class AutohomeParser : ICarParser
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AutohomeParser> _logger;
    private readonly IBrowsingContext _context;

    public string SourceName => "Autohome";
    public string Country => "China";

    public AutohomeParser(HttpClient httpClient, ILogger<AutohomeParser> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Настройка HttpClient для китайского сайта
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
        );
        _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en;q=0.8");

        // Настройка AngleSharp для парсинга HTML
        var configuration = Configuration.Default.WithDefaultLoader();
        _context = BrowsingContext.New(configuration);
    }

    public async Task<IReadOnlyList<ParsedCarData>> ParseListAsync(string url, CancellationToken cancellationToken = default)
    {
        var cars = new List<ParsedCarData>();

        try
        {
            // Загружаем и парсим HTML
            var document = await _context.OpenAsync(url, cancellationToken);
            
            // Находим все блоки с автомобилями
            // Примечание: селекторы CSS нужно адаптировать под реальную структуру Autohome
            var carElements = document.QuerySelectorAll(".car-item, .car-box, [data-type=car]");

            foreach (var carElement in carElements)
            {
                var car = ParseCarElement(carElement, url);
                if (car != null)
                {
                    cars.Add(car);
                }
            }

            _logger.LogInformation("Спаршено {Count} автомобилей со страницы {Url}", cars.Count, url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при парсинге страницы {Url}", url);
            throw;
        }

        return cars;
    }

    public async Task<ParsedCarData?> ParseDetailsAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _context.OpenAsync(url, cancellationToken);
            
            var car = new ParsedCarData
            {
                SourceUrl = url,
                Currency = "CNY"
            };

            // Извлекаем данные из детальной страницы
            // Селекторы нужно адаптировать под реальную структуру
            
            // Название бренда и модели
            var title = document.QuerySelector("h1.title, .car-title")?.TextContent.Trim();
            if (!string.IsNullOrEmpty(title))
            {
                // Парсим "Brand Model Year" из заголовка
                var parts = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    car.BrandName = parts[0];
                    car.ModelName = string.Join(" ", parts.Skip(1).Take(parts.Length - 1));
                }
            }

            // Цена
            var priceElement = document.QuerySelector(".price, .car-price, span.price")?.TextContent;
            if (!string.IsNullOrEmpty(priceElement))
            {
                car.Price = ParsePrice(priceElement);
            }

            // Год
            var yearElement = document.QuerySelector(".year, .car-year")?.TextContent;
            if (!string.IsNullOrEmpty(yearElement) && int.TryParse(yearElement, out var year))
            {
                car.Year = year;
            }

            // Пробег
            var mileageElement = document.QuerySelector(".mileage, .car-mileage")?.TextContent;
            if (!string.IsNullOrEmpty(mileageElement))
            {
                car.Mileage = ParseMileage(mileageElement);
            }

            // Изображение
            car.ImageUrl = document.QuerySelector("img.car-image, .main-image")?.GetAttribute("src");

            // Расположение
            car.Location = document.QuerySelector(".location, .dealer-location")?.TextContent.Trim();

            return car;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при парсинге деталей {Url}", url);
            return null;
        }
    }

    private ParsedCarData? ParseCarElement(IElement element, string baseUrl)
    {
        try
        {
            var car = new ParsedCarData
            {
                Currency = "CNY",
                SourceUrl = baseUrl
            };

            // Извлекаем данные из элемента списка
            var titleElement = element.QuerySelector(".car-title, h3, a.title");
            if (titleElement != null)
            {
                var title = titleElement.TextContent.Trim();
                // Парсим бренд и модель из заголовка
                var parts = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    car.BrandName = parts[0];
                    car.ModelName = string.Join(" ", parts.Skip(1));
                }
            }

            // Цена
            var priceElement = element.QuerySelector(".price, .car-price");
            if (priceElement != null)
            {
                car.Price = ParsePrice(priceElement.TextContent);
            }

            // Ссылка на детальную страницу
            var linkElement = element.QuerySelector("a[href]");
            if (linkElement != null)
            {
                var relativeUrl = linkElement.GetAttribute("href");
                if (!string.IsNullOrEmpty(relativeUrl))
                {
                    car.SourceUrl = new Uri(new Uri(baseUrl), relativeUrl).ToString();
                }
            }

            // Изображение
            var imgElement = element.QuerySelector("img");
            if (imgElement != null)
            {
                car.ImageUrl = imgElement.GetAttribute("src");
            }

            return car;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка при парсинге элемента автомобиля");
            return null;
        }
    }

    private decimal ParsePrice(string priceText)
    {
        // Удаляем символы валюты и пробелы
        var clean = priceText.Replace("¥", "")
                             .Replace("元", "")
                             .Replace(",", "")
                             .Replace("万", "0000") // 1 万 = 10000
                             .Trim();

        if (decimal.TryParse(clean, out var price))
        {
            return price;
        }

        return 0;
    }

    private int ParseMileage(string mileageText)
    {
        // Удаляем символы и парсим число
        var clean = mileageText.Replace("公里", "")
                               .Replace("km", "")
                               .Replace(",", "")
                               .Trim();

        if (int.TryParse(clean, out var mileage))
        {
            return mileage;
        }

        return 0;
    }
}
