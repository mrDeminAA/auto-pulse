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

    // CSS селекторы для Autohome
    private static class Selectors
    {
        // Список автомобилей
        public const string CarListContainer = ".car-list, .car-items";
        public const string CarItem = ".car-item, .car-box, [data-type=car]";
        public const string CarTitle = ".car-title, h3, a.title";
        public const string CarPrice = ".price, .car-price, .red";
        public const string CarImage = "img.car-image, .car-img img";
        public const string CarLink = "a[href*=/car/]";
        
        // Детали автомобиля
        public const string DetailTitle = "h1.title, .car-title";
        public const string DetailPrice = ".price-box, .car-price";
        public const string DetailYear = ".year, .car-year";
        public const string DetailMileage = ".mileage, .car-mileage";
        public const string DetailLocation = ".location, .dealer-address";
        public const string DetailDealer = ".dealer-name, .seller-name";
        
        // Характеристики
        public const string DetailTransmission = ".transmission, [data-item=transmission]";
        public const string DetailEngine = ".engine, [data-item=engine]";
        public const string DetailFuelType = ".fuel, [data-item=fuel]";
        public const string DetailColor = ".color, [data-item=color]";
    }

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
            _logger.LogInformation("Начало парсинга страницы: {Url}", url);

            // Загружаем и парсим HTML
            var document = await _context.OpenAsync(url, cancellationToken);
            
            // Находим все блоки с автомобилями
            var carElements = document.QuerySelectorAll(Selectors.CarItem);

            _logger.LogInformation("Найдено {Count} элементов автомобилей", carElements.Length);

            foreach (var carElement in carElements)
            {
                try
                {
                    var car = ParseCarElement(carElement, url);
                    if (car != null && !string.IsNullOrEmpty(car.BrandName))
                    {
                        cars.Add(car);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка при парсинге элемента автомобиля");
                }
            }

            _logger.LogInformation("Успешно спаршено {Count} автомобилей", cars.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критическая ошибка при парсинге страницы {Url}", url);
            throw;
        }

        return cars;
    }

    public async Task<ParsedCarData?> ParseDetailsAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Парсинг деталей: {Url}", url);

            var document = await _context.OpenAsync(url, cancellationToken);
            
            var car = new ParsedCarData
            {
                SourceUrl = url,
                Currency = "CNY"
            };

            // Название и бренд/модель
            var titleElement = document.QuerySelector(Selectors.DetailTitle);
            if (titleElement != null)
            {
                var title = titleElement.TextContent.Trim();
                ParseTitle(title, car);
            }

            // Цена
            var priceElement = document.QuerySelector(Selectors.DetailPrice);
            if (priceElement != null)
            {
                car.Price = ParsePrice(priceElement.TextContent);
            }

            // Год
            var yearElement = document.QuerySelector(Selectors.DetailYear);
            if (yearElement != null && int.TryParse(yearElement.TextContent.Trim(), out var year))
            {
                car.Year = year;
            }

            // Пробег
            var mileageElement = document.QuerySelector(Selectors.DetailMileage);
            if (mileageElement != null)
            {
                car.Mileage = ParseMileage(mileageElement.TextContent);
            }

            // Расположение
            var locationElement = document.QuerySelector(Selectors.DetailLocation);
            if (locationElement != null)
            {
                car.Location = locationElement.TextContent.Trim();
            }

            // Дилер
            var dealerElement = document.QuerySelector(Selectors.DetailDealer);
            if (dealerElement != null)
            {
                car.DealerName = dealerElement.TextContent.Trim();
            }

            // Характеристики
            car.Transmission = document.QuerySelector(Selectors.DetailTransmission)?.TextContent.Trim();
            car.Engine = document.QuerySelector(Selectors.DetailEngine)?.TextContent.Trim();
            car.FuelType = document.QuerySelector(Selectors.DetailFuelType)?.TextContent.Trim();
            car.Color = document.QuerySelector(Selectors.DetailColor)?.TextContent.Trim();

            // Изображение
            var imgElement = document.QuerySelector("img.main-image, img[src*='car']");
            if (imgElement != null)
            {
                car.ImageUrl = imgElement.GetAttribute("src");
            }

            _logger.LogInformation("Детали спаршены: {Brand} {Model}", car.BrandName, car.ModelName);

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

            // Название/бренд
            var titleElement = element.QuerySelector(Selectors.CarTitle);
            if (titleElement != null)
            {
                var title = titleElement.TextContent.Trim();
                ParseTitle(title, car);
            }

            // Цена
            var priceElement = element.QuerySelector(Selectors.CarPrice);
            if (priceElement != null)
            {
                car.Price = ParsePrice(priceElement.TextContent);
            }

            // Ссылка на детальную страницу
            var linkElement = element.QuerySelector(Selectors.CarLink);
            if (linkElement != null)
            {
                var relativeUrl = linkElement.GetAttribute("href");
                if (!string.IsNullOrEmpty(relativeUrl))
                {
                    car.SourceUrl = new Uri(new Uri(baseUrl), relativeUrl).ToString();
                }
            }

            // Изображение
            var imgElement = element.QuerySelector(Selectors.CarImage);
            if (imgElement != null)
            {
                car.ImageUrl = imgElement.GetAttribute("src");
            }

            return car;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка при парсинге элемента");
            return null;
        }
    }

    private void ParseTitle(string title, ParsedCarData car)
    {
        // Форматы заголовков:
        // "BMW 5 Series 2024"
        // "Audi A6L 2023 2.0T"
        // "Mercedes-Benz E-Class"
        
        var parts = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length >= 2)
        {
            // Первый элемент - бренд
            car.BrandName = parts[0];
            
            // Остальное - модель (ищем год)
            var modelParts = new List<string>();
            for (int i = 1; i < parts.Length; i++)
            {
                if (int.TryParse(parts[i], out var year) && year >= 1900 && year <= DateTime.UtcNow.Year + 1)
                {
                    car.Year = year;
                    break;
                }
                modelParts.Add(parts[i]);
            }
            
            car.ModelName = string.Join(" ", modelParts);
        }
        else if (parts.Length == 1)
        {
            car.BrandName = parts[0];
            car.ModelName = "Unknown";
        }
    }

    private decimal ParsePrice(string priceText)
    {
        if (string.IsNullOrEmpty(priceText))
            return 0;

        // Удаляем символы валюты и пробелы
        var clean = priceText
            .Replace("¥", "")
            .Replace("元", "")
            .Replace("万", "0000") // 1 万 = 10000
            .Replace(",", "")
            .Replace(".", ",") // Китайский формат
            .Trim();

        if (decimal.TryParse(clean, out var price))
        {
            return price;
        }

        // Пробуем найти число в тексте
        var match = System.Text.RegularExpressions.Regex.Match(clean, @"[\d,]+");
        if (match.Success && decimal.TryParse(match.Value, out price))
        {
            return price;
        }

        return 0;
    }

    private int ParseMileage(string mileageText)
    {
        if (string.IsNullOrEmpty(mileageText))
            return 0;

        // Удаляем символы и парсим число
        var clean = mileageText
            .Replace("公里", "")
            .Replace("km", "")
            .Replace("万公里", "0000")
            .Replace(",", "")
            .Trim();

        if (int.TryParse(clean, out var mileage))
        {
            return mileage;
        }

        return 0;
    }
}
