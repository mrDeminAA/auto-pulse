using System.Net.Http;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Application.Parsing;

/// <summary>
/// Парсер для китайского сайта Che168 (www.che168.com)
/// </summary>
public class Che168Parser : ICarParser
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Che168Parser> _logger;
    private readonly IBrowsingContext _context;

    public string SourceName => "Che168";
    public string Country => "China";

    // CSS селекторы для Che168
    private static class Selectors
    {
        // Список автомобилей (на странице дилера)
        public const string CarListContainer = ".carte_list, .car-list, .dealer_car_list";
        public const string CarItem = ".carte_list_li, .car-item, .dealer_car_list_item";
        public const string CarTitle = ".carte_list_li_title, .car-title, a[href*='/dealer/']";
        public const string CarPrice = ".carte_list_li_price, .car-price, .price";
        public const string CarImage = "img.carte_list_li_img, .car-img img, img.lazy";
        public const string CarLink = "a[href*='/dealer/'][href*='.html']";

        // Детали автомобиля
        public const string DetailTitle = "h1.title, .car_title, .main_title";
        public const string DetailPrice = ".price, .car_price, .now_price, .red";
        public const string DetailYear = ".year, .car_year";
        public const string DetailMileage = ".mileage, .car_mileage, [title*='公里']";
        public const string DetailLocation = ".location, .city, .area";
        public const string DetailDealer = ".dealer_name, .dealer_info, .seller";
        public const string DetailContact = ".contact, .phone, .tel";

        // Характеристики
        public const string DetailTransmission = ".transmission, .gear, [data-item='transmission']";
        public const string DetailEngine = ".engine, .displacement, [data-item='engine']";
        public const string DetailFuelType = ".fuel, .fuel_type, [data-item='fuel']";
        public const string DetailColor = ".color, .car_color, [data-item='color']";
        public const string DetailVin = ".vin, .vin_code, .车架号";
    }

    public Che168Parser(HttpClient httpClient, ILogger<Che168Parser> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Настройка HttpClient для китайского сайта
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en;q=0.8");
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36"
        );
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "navigate");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "none");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cache-Control", "max-age=0");

        // Настройка AngleSharp для парсинга HTML
        var configuration = Configuration.Default.WithDefaultLoader();
        _context = BrowsingContext.New(configuration);
    }

    public async Task<IReadOnlyList<ParsedCarData>> ParseListAsync(string url, CancellationToken cancellationToken = default)
    {
        var cars = new List<ParsedCarData>();

        try
        {
            _logger.LogInformation("=== НАЧАЛО ПАРСИНГА СПИСКА Che168 ===");
            _logger.LogInformation("URL: {Url}", url);
            _logger.LogInformation("User-Agent: {UserAgent}", _httpClient.DefaultRequestHeaders.UserAgent.ToString());

            // Загружаем и парсим HTML
            _logger.LogInformation("Загрузка HTML со страницы...");
            var document = await _context.OpenAsync(url, cancellationToken);
            
            _logger.LogInformation("HTML загружен. Status: {Status}, URL: {Url}", document.Url, document.Url);
            _logger.LogDebug("HTML размер: {Size} байт", document.ToHtml().Length);

            // Сохраняем HTML для отладки
            var htmlContent = document.ToHtml();
            _logger.LogDebug("HTML содержимое (первые 2000 символов): {HtmlPreview}", 
                htmlContent.Length > 2000 ? htmlContent.Substring(0, 2000) : htmlContent);

            // Находим все блоки с автомобилями
            _logger.LogInformation("Поиск элементов по селектору: {Selector}", Selectors.CarItem);
            var carElements = document.QuerySelectorAll(Selectors.CarItem);

            _logger.LogInformation("Найдено {Count} элементов автомобилей", carElements.Length);

            // Логируем найденные элементы
            for (int i = 0; i < carElements.Length; i++)
            {
                var element = carElements[i];
                _logger.LogDebug("Элемент #{Index}: Tag={Tag}, Class={Class}, HTML={Html}", 
                    i, 
                    element.TagName, 
                    element.GetAttribute("class"),
                    element.ToHtml().Length > 300 ? element.ToHtml().Substring(0, 300) + "..." : element.ToHtml());
            }

            // Если не найдено элементов по основному селектору, пробуем альтернативные
            if (carElements.Length == 0)
            {
                _logger.LogWarning("Не найдено элементов по основному селектору. Пробуем альтернативные...");
                
                var alternativeSelectors = new[] { ".carte_list_li", ".car-item", ".dealer_car_list_item", "li", "div" };
                foreach (var altSelector in alternativeSelectors)
                {
                    var altElements = document.QuerySelectorAll(altSelector);
                    _logger.LogDebug("Селектор {Selector}: найдено {Count} элементов", altSelector, altElements.Length);
                }
                
                // Пробуем найти любой контейнер со списком
                var listContainer = document.QuerySelector(Selectors.CarListContainer);
                if (listContainer != null)
                {
                    _logger.LogInformation("Найден контейнер списка: {Container}", listContainer.ToHtml().Substring(0, Math.Min(200, listContainer.ToHtml().Length)));
                }
            }

            foreach (var carElement in carElements)
            {
                try
                {
                    _logger.LogDebug("Парсинг элемента автомобиля...");
                    var car = ParseCarElement(carElement, url);
                    if (car != null && !string.IsNullOrEmpty(car.BrandName))
                    {
                        _logger.LogInformation("✓ Спаршен автомобиль: {Brand} {Model}, Цена: {Price} {Currency}", 
                            car.BrandName, car.ModelName, car.Price, car.Currency);
                        cars.Add(car);
                    }
                    else
                    {
                        _logger.LogWarning("✗ Элемент не распознан как автомобиль (BrandName пустой)");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка при парсинге элемента автомобиля. Element HTML: {Html}", 
                        carElement.ToHtml().Length > 200 ? carElement.ToHtml().Substring(0, 200) + "..." : carElement.ToHtml());
                }
            }

            _logger.LogInformation("=== ЗАВЕРШЕНИЕ ПАРСИНГА СПИСКА ===");
            _logger.LogInformation("Успешно спаршено {Count} автомобилей из {Total} элементов", cars.Count, carElements.Length);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "КРИТИЧЕСКАЯ ОШИБКА при парсинге страницы {Url}", url);
            _logger.LogCritical("Тип ошибки: {ErrorType}, Message: {Message}", ex.GetType().Name, ex.Message);
            throw;
        }

        return cars;
    }

    public async Task<ParsedCarData?> ParseDetailsAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("=== НАЧАЛО ПАРСИНГА ДЕТАЛЕЙ Che168 ===");
            _logger.LogInformation("URL: {Url}", url);

            _logger.LogInformation("Загрузка HTML со страницы...");
            var document = await _context.OpenAsync(url, cancellationToken);
            
            _logger.LogInformation("HTML загружен. Status: {Status}", document.Url);
            _logger.LogDebug("HTML размер: {Size} байт", document.ToHtml().Length);

            var car = new ParsedCarData
            {
                SourceUrl = url,
                Currency = "CNY"
            };

            // Название и бренд/модель
            _logger.LogDebug("Поиск заголовка по селектору: {Selector}", Selectors.DetailTitle);
            var titleElement = document.QuerySelector(Selectors.DetailTitle);
            if (titleElement != null)
            {
                var title = titleElement.TextContent.Trim();
                _logger.LogInformation("Найден заголовок: '{Title}'", title);
                ParseTitle(title, car);
                _logger.LogDebug("Распознано: Brand={Brand}, Model={Model}, Year={Year}", 
                    car.BrandName, car.ModelName, car.Year);
            }
            else
            {
                _logger.LogWarning("Заголовок не найден");
            }

            // Цена
            _logger.LogDebug("Поиск цены по селектору: {Selector}", Selectors.DetailPrice);
            var priceElement = document.QuerySelector(Selectors.DetailPrice);
            if (priceElement != null)
            {
                var priceText = priceElement.TextContent.Trim();
                _logger.LogDebug("Текст цены: '{PriceText}'", priceText);
                car.Price = ParsePrice(priceText);
                _logger.LogInformation("Цена: {Price} {Currency}", car.Price, car.Currency);
            }
            else
            {
                _logger.LogWarning("Цена не найдена");
            }

            // Год
            _logger.LogDebug("Поиск года по селектору: {Selector}", Selectors.DetailYear);
            var yearElement = document.QuerySelector(Selectors.DetailYear);
            if (yearElement != null && int.TryParse(yearElement.TextContent.Trim(), out var year))
            {
                car.Year = year;
                _logger.LogDebug("Год: {Year}", car.Year);
            }
            else
            {
                _logger.LogDebug("Год не найден или не распознан");
            }

            // Пробег
            _logger.LogDebug("Поиск пробега по селектору: {Selector}", Selectors.DetailMileage);
            var mileageElement = document.QuerySelector(Selectors.DetailMileage);
            if (mileageElement != null)
            {
                var mileageText = mileageElement.TextContent.Trim();
                _logger.LogDebug("Текст пробега: '{MileageText}'", mileageText);
                car.Mileage = ParseMileage(mileageText);
                _logger.LogInformation("Пробег: {Mileage} км", car.Mileage);
            }
            else
            {
                _logger.LogDebug("Пробег не найден");
            }

            // Расположение
            _logger.LogDebug("Поиск расположения по селектору: {Selector}", Selectors.DetailLocation);
            var locationElement = document.QuerySelector(Selectors.DetailLocation);
            if (locationElement != null)
            {
                car.Location = locationElement.TextContent.Trim();
                _logger.LogInformation("Расположение: {Location}", car.Location);
            }
            else
            {
                _logger.LogDebug("Расположение не найдено");
            }

            // Дилер
            _logger.LogDebug("Поиск дилера по селектору: {Selector}", Selectors.DetailDealer);
            var dealerElement = document.QuerySelector(Selectors.DetailDealer);
            if (dealerElement != null)
            {
                car.DealerName = dealerElement.TextContent.Trim();
                _logger.LogInformation("Дилер: {DealerName}", car.DealerName);
            }
            else
            {
                _logger.LogDebug("Дилер не найден");
            }

            // Контакты
            _logger.LogDebug("Поиск контактов по селектору: {Selector}", Selectors.DetailContact);
            var contactElement = document.QuerySelector(Selectors.DetailContact);
            if (contactElement != null)
            {
                car.DealerContact = contactElement.TextContent.Trim();
                _logger.LogInformation("Контакты: {Contact}", car.DealerContact);
            }
            else
            {
                _logger.LogDebug("Контакты не найдены");
            }

            // VIN
            _logger.LogDebug("Поиск VIN по селектору: {Selector}", Selectors.DetailVin);
            var vinElement = document.QuerySelector(Selectors.DetailVin);
            if (vinElement != null)
            {
                car.Vin = vinElement.TextContent.Trim();
                _logger.LogInformation("VIN: {Vin}", car.Vin);
            }
            else
            {
                _logger.LogDebug("VIN не найден");
            }

            // Характеристики
            _logger.LogInformation("Парсинг характеристик...");
            
            _logger.LogDebug("Поиск трансмиссии...");
            var transmissionElement = document.QuerySelector(Selectors.DetailTransmission);
            car.Transmission = transmissionElement?.TextContent.Trim();
            _logger.LogDebug("Трансмиссия: {Transmission}", car.Transmission ?? "не найдена");

            _logger.LogDebug("Поиск двигателя...");
            var engineElement = document.QuerySelector(Selectors.DetailEngine);
            car.Engine = engineElement?.TextContent.Trim();
            _logger.LogDebug("Двигатель: {Engine}", car.Engine ?? "не найден");

            _logger.LogDebug("Поиск типа топлива...");
            var fuelElement = document.QuerySelector(Selectors.DetailFuelType);
            car.FuelType = fuelElement?.TextContent.Trim();
            _logger.LogDebug("Тип топлива: {FuelType}", car.FuelType ?? "не найден");

            _logger.LogDebug("Поиск цвета...");
            var colorElement = document.QuerySelector(Selectors.DetailColor);
            car.Color = colorElement?.TextContent.Trim();
            _logger.LogDebug("Цвет: {Color}", car.Color ?? "не найден");

            // Изображение
            _logger.LogDebug("Поиск изображения...");
            var imgElement = document.QuerySelector("img.main-image, img[src*='car'], img.lazy");
            if (imgElement != null)
            {
                car.ImageUrl = imgElement.GetAttribute("src");
                _logger.LogInformation("Изображение: {ImageUrl}", car.ImageUrl);
            }
            else
            {
                _logger.LogDebug("Изображение не найдено");
            }

            _logger.LogInformation("=== ЗАВЕРШЕНИЕ ПАРСИНГА ДЕТАЛЕЙ ===");
            _logger.LogInformation("Результат: {Brand} {Model} ({Year}), Цена: {Price} {Currency}", 
                car.BrandName, car.ModelName, car.Year > 0 ? car.Year.ToString() : "н/д", car.Price, car.Currency);

            return car;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "КРИТИЧЕСКАЯ ОШИБКА при парсинге деталей {Url}", url);
            _logger.LogCritical("Тип ошибки: {ErrorType}, Message: {Message}, StackTrace: {StackTrace}", 
                ex.GetType().Name, ex.Message, ex.StackTrace);
            return null;
        }
    }

    private ParsedCarData? ParseCarElement(IElement element, string baseUrl)
    {
        try
        {
            _logger.LogDebug(">>> ParseCarElement: Начало");
            _logger.LogDebug("Element: Tag={Tag}, Class={Class}, Id={Id}", 
                element.TagName, 
                element.GetAttribute("class"), 
                element.GetAttribute("id"));

            var car = new ParsedCarData
            {
                Currency = "CNY",
                SourceUrl = baseUrl
            };

            // Название/бренд
            _logger.LogDebug("Поиск заголовка по селектору: {Selector}", Selectors.CarTitle);
            var titleElement = element.QuerySelector(Selectors.CarTitle);
            if (titleElement != null)
            {
                var title = titleElement.TextContent.Trim();
                _logger.LogDebug("Заголовок: '{Title}'", title);
                ParseTitle(title, car);
                _logger.LogDebug("Распознано: Brand={Brand}, Model={Model}, Year={Year}", 
                    car.BrandName, car.ModelName, car.Year);
            }
            else
            {
                _logger.LogDebug("Заголовок не найден");
            }

            // Цена
            _logger.LogDebug("Поиск цены по селектору: {Selector}", Selectors.CarPrice);
            var priceElement = element.QuerySelector(Selectors.CarPrice);
            if (priceElement != null)
            {
                var priceText = priceElement.TextContent.Trim();
                _logger.LogDebug("Текст цены: '{PriceText}'", priceText);
                car.Price = ParsePrice(priceText);
                _logger.LogDebug("Цена: {Price}", car.Price);
            }
            else
            {
                _logger.LogDebug("Цена не найдена");
            }

            // Ссылка на детальную страницу
            _logger.LogDebug("Поиск ссылки...");
            var linkElement = element.QuerySelector(Selectors.CarLink);
            if (linkElement != null)
            {
                var relativeUrl = linkElement.GetAttribute("href");
                _logger.LogDebug("Найдена ссылка: {Href}", relativeUrl);
                if (!string.IsNullOrEmpty(relativeUrl))
                {
                    car.SourceUrl = new Uri(new Uri(baseUrl), relativeUrl).ToString();
                    _logger.LogDebug("Полный URL: {Url}", car.SourceUrl);
                }
            }
            else
            {
                _logger.LogDebug("Ссылка не найдена");
            }

            // Изображение
            _logger.LogDebug("Поиск изображения...");
            var imgElement = element.QuerySelector(Selectors.CarImage);
            if (imgElement != null)
            {
                car.ImageUrl = imgElement.GetAttribute("src");
                _logger.LogDebug("Изображение: {ImageUrl}", car.ImageUrl);
            }
            else
            {
                _logger.LogDebug("Изображение не найдено");
            }

            _logger.LogDebug("<<< ParseCarElement: Завершение. Brand={Brand}, Model={Model}", 
                car.BrandName, car.ModelName);

            return car;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка при парсинге элемента. Element HTML: {Html}", 
                element.ToHtml().Length > 300 ? element.ToHtml().Substring(0, 300) + "..." : element.ToHtml());
            return null;
        }
    }

    private void ParseTitle(string title, ParsedCarData car)
    {
        _logger.LogDebug("ParseTitle: Входной заголовок = '{Title}'", title);
        
        // Форматы заголовков Che168:
        // "奥迪 A6L 2021 款 40 TFSI 豪华动感型"
        // "宝马 5 系 2022 款 530Li 领先型 M 运动套装"
        // "Mercedes-Benz E-Class 2023"

        var parts = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        _logger.LogDebug("ParseTitle: Разбито на {Count} частей: {Parts}", 
            parts.Length, string.Join(", ", parts));

        if (parts.Length >= 2)
        {
            // Первый элемент - бренд (китайский или английский)
            car.BrandName = parts[0];
            _logger.LogDebug("ParseTitle: Бренд = {Brand}", car.BrandName);

            // Остальное - модель (ищем год)
            var modelParts = new List<string>();
            for (int i = 1; i < parts.Length; i++)
            {
                if (int.TryParse(parts[i], out var year) && year >= 1900 && year <= DateTime.UtcNow.Year + 1)
                {
                    car.Year = year;
                    _logger.LogDebug("ParseTitle: Найден год = {Year} на позиции {Index}", year, i);
                    break;
                }
                modelParts.Add(parts[i]);
            }

            car.ModelName = string.Join(" ", modelParts);
            _logger.LogDebug("ParseTitle: Модель = {Model}", car.ModelName);
        }
        else if (parts.Length == 1)
        {
            car.BrandName = parts[0];
            car.ModelName = "Unknown";
            _logger.LogDebug("ParseTitle: Только бренд = {Brand}, модель неизвестна", car.BrandName);
        }
        else
        {
            _logger.LogWarning("ParseTitle: Пустой заголовок");
        }
        
        _logger.LogDebug("ParseTitle: Результат - Brand={Brand}, Model={Model}, Year={Year}", 
            car.BrandName, car.ModelName, car.Year);
    }

    private decimal ParsePrice(string priceText)
    {
        _logger.LogDebug("ParsePrice: Входной текст = '{PriceText}'", priceText);
        
        if (string.IsNullOrEmpty(priceText))
        {
            _logger.LogDebug("ParsePrice: Пустой текст, возврат 0");
            return 0;
        }

        // Удаляем символы валюты и пробелы
        var clean = priceText
            .Replace("¥", "")
            .Replace("元", "")
            .Replace("万", "0000") // 1 万 = 10000
            .Replace(",", "")
            .Replace(".", ",") // Китайский формат
            .Replace("￥", "")
            .Replace("W", "0000") // W = 万
            .Trim();

        _logger.LogDebug("ParsePrice: Очищенный текст = '{Clean}'", clean);

        if (decimal.TryParse(clean, out var price))
        {
            _logger.LogDebug("ParsePrice: Распознано число = {Price}", price);
            return price;
        }

        // Пробуем найти число в тексте
        var match = System.Text.RegularExpressions.Regex.Match(clean, @"[\d,]+");
        if (match.Success && decimal.TryParse(match.Value, out price))
        {
            _logger.LogDebug("ParsePrice: Найдено по regex = {Price} (Pattern: {Pattern})", price, match.Value);
            return price;
        }

        _logger.LogWarning("ParsePrice: Не удалось распознать цену из '{PriceText}'", priceText);
        return 0;
    }

    private int ParseMileage(string mileageText)
    {
        _logger.LogDebug("ParseMileage: Входной текст = '{MileageText}'", mileageText);
        
        if (string.IsNullOrEmpty(mileageText))
        {
            _logger.LogDebug("ParseMileage: Пустой текст, возврат 0");
            return 0;
        }

        // Удаляем символы и парсим число
        var clean = mileageText
            .Replace("公里", "")
            .Replace("km", "")
            .Replace("万公里", "0000")
            .Replace("千米", "")
            .Replace(",", "")
            .Trim();

        _logger.LogDebug("ParseMileage: Очищенный текст = '{Clean}'", clean);

        if (int.TryParse(clean, out var mileage))
        {
            _logger.LogDebug("ParseMileage: Распознано число = {Mileage}", mileage);
            return mileage;
        }

        _logger.LogDebug("ParseMileage: Не удалось распознать пробег");
        return 0;
    }
}
