using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Parsing;

/// <summary>
/// Парсер Che168 через прямое API
/// </summary>
public class Che168ApiParser
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Che168ApiParser> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    // API endpoint
    private const string ApiBaseUrl = "https://api2scsou.che168.com/api/v11/search";

    // Brand IDs
    public static class BrandIds
    {
        public const string Audi = "33";
        public const string Bmw = "56";
        public const string Mercedes = "57";
    }

    public Che168ApiParser(HttpClient httpClient, ILogger<Che168ApiParser> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Настраиваем заголовки для мобильных устройств
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1"
        );
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/plain, */*");
        _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9");
    }

    /// <summary>
    /// Получить список автомобилей
    /// </summary>
    public async Task<IReadOnlyList<ParsedCarData>> ParseAsync(
        string brandId,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Начало парсинга Che168: brand={Brand}, page={Page}", brandId, pageIndex);

        try
        {
            var apiUrl = BuildApiUrl(brandId, pageIndex, pageSize);
            _logger.LogDebug("API URL: {Url}", apiUrl);

            var response = await _httpClient.GetAsync(apiUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Получен JSON, размер: {Size} байт", jsonContent.Length);

            var apiResponse = JsonSerializer.Deserialize<Che168ApiResponse>(jsonContent, _jsonOptions);

            if (apiResponse == null || apiResponse.ReturnCode != 0)
            {
                _logger.LogWarning("API вернул ошибку: code={Code}, message={Message}",
                    apiResponse?.ReturnCode, apiResponse?.Message);
                return Array.Empty<ParsedCarData>();
            }

            var cars = ParseCarList(apiResponse.Result?.CarList);
            _logger.LogInformation("Успешно спаршено {Count} автомобилей", cars.Count);

            return cars;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при парсинге Che168");
            throw;
        }
    }

    /// <summary>
    /// Получить все автомобили с пагинацией
    /// </summary>
    public async IAsyncEnumerable<ParsedCarData> ParseAllAsync(
        string brandId,
        int maxPages = 100,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int page = 1;
        int totalParsed = 0;

        while (page <= maxPages && !cancellationToken.IsCancellationRequested)
        {
            var cars = await ParseAsync(brandId, page, 10, cancellationToken);

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

            // Небольшая задержка между запросами
            await Task.Delay(100, cancellationToken);
        }
    }

    private string BuildApiUrl(string brandId, int pageIndex, int pageSize)
    {
        // Минимально необходимые параметры
        var parameters = new Dictionary<string, string>
        {
            { "pageindex", pageIndex.ToString() },
            { "pagesize", pageSize.ToString() },
            { "brandid", brandId },
            { "seriesyearid", "" },
            { "ishideback", "1" },
            { "srecom", "2" },
            { "personalizedpush", "1" },
            { "cid", "0" },
            { "car_area", "1" },
            { "iscxcshowed", "-1" },
            { "scene_no", "12" },
            { "pageid", "1771440367_2602" },
            { "testtype", "X" },
            { "test102223", "X" },
            { "filtertype", "0" },
            { "ssnew", "1" },
            { "deviceid", "d9457747-2d23-638c-041b-83eba3487cfe" },
            { "userid", "0" },
            { "s_pid", "0" },
            { "s_cid", "0" },
            { "_appid", "2sc.m" },
            { "_subappid", "" },
            { "v", "11.41.5" }
            // _sign не добавляем - он динамический
        };

        var queryString = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return $"{ApiBaseUrl}?{queryString}";
    }

    private List<ParsedCarData> ParseCarList(List<Che168CarDto>? carList)
    {
        var result = new List<ParsedCarData>();

        if (carList == null)
            return result;

        foreach (var dto in carList)
        {
            try
            {
                var car = MapToCarData(dto);
                if (car != null)
                {
                    result.Add(car);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка при парсинге автомобиля {Infoid}", dto.Infoid);
            }
        }

        return result;
    }

    private ParsedCarData? MapToCarData(Che168CarDto dto)
    {
        if (string.IsNullOrEmpty(dto.Carname))
            return null;

        var (brand, model, year) = ParseCarName(dto.Carname);

        // Формируем URL изображения
        var imageUrl = BuildImageUrl(dto);

        return new ParsedCarData
        {
            Id = dto.Infoid,
            Brand = brand,
            Model = model,
            FullName = dto.Carname,
            Year = year,
            Price = ParsePrice(dto.Price),
            Mileage = ParseMileage(dto.Mileage),
            City = dto.Cname ?? string.Empty,
            Dealer = dto.DealerLevel ?? string.Empty,
            Displacement = dto.Displacement,
            ImageUrl = imageUrl,
            SourceUrl = $"https://www.che168.com/home?infoid={dto.Infoid}",
            Source = "Che168"
        };
    }

    private string BuildImageUrl(Che168CarDto dto)
    {
        // Приоритет: ImageUrl_800 > Imageurl > заглушка
        if (!string.IsNullOrEmpty(dto.ImageUrl_800))
        {
            return dto.ImageUrl_800.StartsWith("http") 
                ? dto.ImageUrl_800 
                : $"https:{dto.ImageUrl_800}";
        }

        if (!string.IsNullOrEmpty(dto.Imageurl))
        {
            return dto.Imageurl.StartsWith("http") 
                ? dto.Imageurl 
                : $"https:{dto.Imageurl}";
        }

        // Заглушка если нет изображения
        return $"https://placehold.co/600x400/003d82/ffffff?text={Uri.EscapeDataString(dto.Carname ?? "Car")}";
    }

    private (string brand, string model, int year) ParseCarName(string carName)
    {
        // Пример: "奥迪 Q3 2024 款 40 TFSI 时尚动感型"
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

            // Ищем год (например, "2024 款")
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

    private decimal ParsePrice(string? priceStr)
    {
        if (string.IsNullOrEmpty(priceStr))
            return 0;

        // Цена в 万 (10k юаней)
        if (decimal.TryParse(priceStr, out var price))
        {
            return price * 10000;
        }

        return 0;
    }

    private int ParseMileage(string? mileageStr)
    {
        if (string.IsNullOrEmpty(mileageStr))
            return 0;

        // Пробег в 万 км
        if (decimal.TryParse(mileageStr, out var mileage))
        {
            return (int)(mileage * 10000);
        }

        return 0;
    }
}
