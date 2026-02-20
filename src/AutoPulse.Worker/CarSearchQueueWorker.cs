using AutoPulse.Domain;
using AutoPulse.Infrastructure;
using AutoPulse.Infrastructure.Services;
using AutoPulse.Parsing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Worker;

/// <summary>
/// Фоновый сервис для обработки умной очереди парсинга
/// </summary>
public class CarSearchQueueWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CarSearchQueueWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public CarSearchQueueWorker(
        IServiceProvider serviceProvider,
        ILogger<CarSearchQueueWorker> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Car Search Queue Worker запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessQueueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка в цикле обработки очереди");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var currencyService = scope.ServiceProvider.GetRequiredService<ICurrencyConversionService>();

        // Получаем задачи из очереди, готовые к выполнению
        var queues = await dbContext.CarSearchQueues
            .Include(q => q.Brand)
            .Include(q => q.Model)
            .Include(q => q.UserSearches)
            .Where(q => q.Status == QueueStatus.Pending ||
                       (q.Status == QueueStatus.Completed && q.NextParseAt <= DateTime.UtcNow))
            .OrderByDescending(q => q.Priority)
            .Take(10)
            .ToListAsync(cancellationToken);

        if (!queues.Any())
        {
            _logger.LogDebug("Очередь пуста, ожидание {Interval}", _checkInterval);
            await Task.Delay(_checkInterval, cancellationToken);
            return;
        }

        _logger.LogInformation("Найдено {Count} задач в очереди", queues.Count);

        foreach (var queue in queues)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                await ProcessQueueItemAsync(dbContext, currencyService, queue, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки задачи очереди {QueueId}", queue.Id);
                queue.FailParsing(ex.Message);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task ProcessQueueItemAsync(
        ApplicationDbContext dbContext,
        ICurrencyConversionService currencyService,
        CarSearchQueue queue,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Обработка очереди {QueueId}: {Brand} {Model} ({YearFrom}-{YearTo}), Regions={Regions}, Priority={Priority}",
            queue.Id, queue.Brand?.Name, queue.Model?.Name, queue.YearFrom, queue.YearTo, queue.Regions, queue.Priority);

        // Блокируем задачу (чтобы другие воркеры не обрабатывали)
        queue.StartParsing();
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var allCars = new List<ParsedCarData>();

            // Определяем регионы для парсинга
            var regions = ParseRegions(queue.Regions);

            foreach (var region in regions)
            {
                var cars = await ParseRegionAsync(dbContext, queue, region, cancellationToken);
                allCars.AddRange(cars);

                _logger.LogInformation("Регион {Region}: распаршено {Count} автомобилей", region, cars.Count);

                // Небольшая задержка между регионами
                await Task.Delay(1000, cancellationToken);
            }

            // Фильтруем по году
            var filteredCars = allCars
                .Where(c => c.Year >= queue.YearFrom && c.Year <= queue.YearTo)
                .ToList();

            _logger.LogInformation("Всего распаршено {Count} автомобилей (после фильтрации по годам)", filteredCars.Count);

            if (filteredCars.Any())
            {
                // Сохраняем автомобили
                var storageLogger = _serviceProvider.GetRequiredService<ILogger<CarStorageService>>();
                var storageService = new CarStorageService(dbContext, currencyService, storageLogger);
                var savedCount = await storageService.SaveCarsForQueueAsync(filteredCars, queue.Id, cancellationToken);

                _logger.LogInformation("Сохранено {Count} автомобилей", savedCount);
            }

            // Завершаем успешно
            queue.CompleteParsing(filteredCars.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка парсинга для очереди {QueueId}", queue.Id);
            queue.FailParsing(ex.Message);
            throw;
        }
        finally
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<List<ParsedCarData>> ParseRegionAsync(
        ApplicationDbContext dbContext,
        CarSearchQueue queue,
        string region,
        CancellationToken cancellationToken)
    {
        var cars = new List<ParsedCarData>();

        switch (region.ToLowerInvariant())
        {
            case "china":
                cars = await ParseChinaAsync(dbContext, queue, cancellationToken);
                break;

            case "europe":
                cars = await ParseEuropeAsync(dbContext, queue, cancellationToken);
                break;

            case "usa":
                cars = await ParseUsaAsync(dbContext, queue, cancellationToken);
                break;

            default:
                _logger.LogWarning("Неизвестный регион: {Region}", region);
                break;
        }

        return cars;
    }

    private async Task<List<ParsedCarData>> ParseChinaAsync(
        ApplicationDbContext dbContext,
        CarSearchQueue queue,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Парсинг Китая: {Brand} {Model}", queue.Brand?.Name, queue.Model?.Name);

        var brandId = GetBrandIdForApi(queue.Brand?.Name);
        if (string.IsNullOrEmpty(brandId))
        {
            _logger.LogWarning("Не найден Brand ID для {Brand}", queue.Brand?.Name);
            return new List<ParsedCarData>();
        }

        // Используем Playwright парсер для mobile версии
        var parser = _serviceProvider.GetRequiredService<Che168PlaywrightParser>();
        
        var cars = new List<ParsedCarData>();
        await foreach (var car in parser.ParseAllAsync("https://m.che168.com/carlist/index?pvareaid=111478", 10, 100, cancellationToken))
        {
            cars.Add(car);
            if (cars.Count >= 100)
                break;
        }

        return cars;
    }

    private async Task<List<ParsedCarData>> ParseEuropeAsync(
        ApplicationDbContext dbContext,
        CarSearchQueue queue,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Парсинг Европы (mobile.de): {Brand} {Model}", queue.Brand?.Name, queue.Model?.Name);

        var parser = _serviceProvider.GetRequiredService<MobileDeParser>();
        
        // Формируем URL для mobile.de
        var searchUrl = BuildMobileDeUrl(queue.Brand?.Name, queue.Model?.Name);
        
        if (string.IsNullOrEmpty(searchUrl))
        {
            _logger.LogWarning("Не удалось сформировать URL для mobile.de");
            return new List<ParsedCarData>();
        }

        var cars = new List<ParsedCarData>();
        await foreach (var carDto in parser.ParseAllAsync(searchUrl, 10, 100, cancellationToken))
        {
            // Конвертируем в ParsedCarData
            var car = ConvertMobileDeToParsedCarData(carDto);
            if (car != null)
            {
                cars.Add(car);
            }
        }

        return cars;
    }

    private async Task<List<ParsedCarData>> ParseUsaAsync(
        ApplicationDbContext dbContext,
        CarSearchQueue queue,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Парсинг США (cars.com): {Brand} {Model}", queue.Brand?.Name, queue.Model?.Name);

        var parser = _serviceProvider.GetRequiredService<CarsComParser>();
        
        // Формируем URL для cars.com
        var searchUrl = BuildCarsComUrl(queue.Brand?.Name, queue.Model?.Name);
        
        if (string.IsNullOrEmpty(searchUrl))
        {
            _logger.LogWarning("Не удалось сформировать URL для cars.com");
            return new List<ParsedCarData>();
        }

        var cars = new List<ParsedCarData>();
        await foreach (var carDto in parser.ParseAllAsync(searchUrl, 10, 100, cancellationToken))
        {
            // Конвертируем в ParsedCarData
            var car = ConvertCarsComToParsedCarData(carDto);
            if (car != null)
            {
                cars.Add(car);
            }
        }

        return cars;
    }

    private List<string> ParseRegions(string regionsJson)
    {
        if (string.IsNullOrEmpty(regionsJson))
            return new List<string> { "china" }; // По умолчанию только Китай

        try
        {
            // Парсим JSON массив регионов
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(regionsJson) 
                ?? new List<string> { "china" };
        }
        catch
        {
            // Если не удалось распарсить, пробуем как строку через запятую
            return regionsJson.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim().ToLowerInvariant())
                .ToList();
        }
    }

    private string? BuildMobileDeUrl(string? brandName, string? modelName)
    {
        if (string.IsNullOrEmpty(brandName))
            return null;

        // Словарь брендов для mobile.de
        var brandMakes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Audi", "audi" },
            { "BMW", "bmw" },
            { "Mercedes-Benz", "mercedes-benz" },
            { "Volkswagen", "volkswagen" },
            { "Toyota", "toyota" },
            { "Honda", "honda" },
            { "Nissan", "nissan" },
            { "Mazda", "mazda" },
            { "Lexus", "lexus" },
            { "Porsche", "porsche" }
        };

        if (!brandMakes.TryGetValue(brandName, out var make))
            return null;

        var baseUrl = $"https://www.mobile.de/autos?make={make}";

        if (!string.IsNullOrEmpty(modelName))
        {
            var model = modelName.ToLowerInvariant().Replace(" ", "-");
            baseUrl += $"&model={model}";
        }

        // Добавляем параметры для поиска по Европе
        baseUrl += "&damagedLst=false&isSearchRequest=true&sfct=false";

        return baseUrl;
    }

    private string? BuildCarsComUrl(string? brandName, string? modelName)
    {
        if (string.IsNullOrEmpty(brandName))
            return null;

        // Словарь брендов для cars.com
        var brandSlugs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Audi", "audi" },
            { "BMW", "bmw" },
            { "Mercedes-Benz", "mercedes-benz" },
            { "Volkswagen", "volkswagen" },
            { "Toyota", "toyota" },
            { "Honda", "honda" },
            { "Nissan", "nissan" },
            { "Mazda", "mazda" },
            { "Lexus", "lexus" },
            { "Porsche", "porsche" }
        };

        if (!brandSlugs.TryGetValue(brandName, out var slug))
            return null;

        var baseUrl = $"https://www.cars.com/shopping/{slug}/";

        if (!string.IsNullOrEmpty(modelName))
        {
            var model = modelName.ToLowerInvariant().Replace(" ", "-");
            baseUrl += $"{model}/";
        }

        baseUrl += "?page_size=20&zip=10001&distance=99999";

        return baseUrl;
    }

    private ParsedCarData? ConvertMobileDeToParsedCarData(MobileDeCarDto dto)
    {
        if (string.IsNullOrEmpty(dto.Title))
            return null;

        var year = 0;
        if (!string.IsNullOrEmpty(dto.Year))
        {
            int.TryParse(dto.Year, out year);
        }

        var price = 0m;
        if (!string.IsNullOrEmpty(dto.Price))
        {
            decimal.TryParse(dto.Price, out price);
        }

        var mileage = 0;
        if (!string.IsNullOrEmpty(dto.Mileage))
        {
            var cleanMileage = dto.Mileage.Replace("km", "").Replace(".", "").Replace(",", "").Trim();
            int.TryParse(cleanMileage, out mileage);
        }

        return new ParsedCarData
        {
            Id = dto.Url?.GetHashCode() ?? 0,
            Brand = dto.Brand ?? "",
            Model = dto.Model ?? "",
            FullName = dto.Title,
            Year = year,
            Price = price,
            Mileage = mileage,
            City = dto.City ?? "",
            Dealer = dto.Country ?? "",
            ImageUrl = dto.ImageUrl,
            SourceUrl = dto.Url ?? "",
            Source = "Mobile.de",
            ParsedAt = DateTime.UtcNow
        };
    }

    private ParsedCarData? ConvertCarsComToParsedCarData(CarsComCarDto dto)
    {
        if (string.IsNullOrEmpty(dto.Title))
            return null;

        var year = 0;
        if (!string.IsNullOrEmpty(dto.Year))
        {
            int.TryParse(dto.Year, out year);
        }

        var price = 0m;
        if (!string.IsNullOrEmpty(dto.Price))
        {
            decimal.TryParse(dto.Price, out price);
        }

        var mileage = 0;
        if (!string.IsNullOrEmpty(dto.Mileage))
        {
            int.TryParse(dto.Mileage, out mileage);
        }

        return new ParsedCarData
        {
            Id = dto.Url?.GetHashCode() ?? 0,
            Brand = dto.Brand ?? "",
            Model = dto.Model ?? "",
            FullName = dto.Title,
            Year = year,
            Price = price,
            Mileage = mileage,
            City = dto.City ?? "",
            Dealer = dto.DealerName ?? "",
            ImageUrl = dto.ImageUrl,
            SourceUrl = dto.Url ?? "",
            Source = "Cars.com",
            ParsedAt = DateTime.UtcNow
        };
    }

    private string? GetBrandIdForApi(string? brandName)
    {
        if (string.IsNullOrEmpty(brandName))
            return null;

        // Brand ID для API che168
        var brandIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Audi", "33" },
            { "BMW", "56" },
            { "Mercedes-Benz", "57" },
            { "Volkswagen", "60" },
            { "Toyota", "54" },
            { "Honda", "59" },
            { "Nissan", "55" },
            { "Mazda", "66" },
            { "Lexus", "65" },
            { "Porsche", "61" }
        };

        return brandIds.TryGetValue(brandName, out var id) ? id : null;
    }

    private string? GetBrandSlug(string? brandName)
    {
        if (string.IsNullOrEmpty(brandName))
            return null;

        // Словарь соответствия названий брендов slug'ам на che168
        var brandSlugs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "奥迪", "aodi" },      // Audi
            { "宝马", "baoma" },     // BMW
            { "奔驰", "benchi" },    // Mercedes-Benz
            { "大众", "dazhong" },   // Volkswagen
            { "丰田", "fengtian" },  // Toyota
            { "本田", "bentian" },   // Honda
            { "日产", "richan" },    // Nissan
            { "马自达", "mazida" },  // Mazda
            { "雷克萨斯", "leikesasi" }, // Lexus
            { "保时捷", "baoshijie" }   // Porsche
        };

        return brandSlugs.TryGetValue(brandName, out var slug) ? slug : brandName.ToLowerInvariant();
    }

    private string? GetModelSlug(string? modelName)
    {
        if (string.IsNullOrEmpty(modelName))
            return null;

        // Для моделей используем транслитерацию или готовый словарь
        var modelSlugs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "A3", "a3" },
            { "A4", "a4" },
            { "A4L", "a4l" },
            { "A6", "a6" },
            { "A6L", "a6l" },
            { "Q5", "q5" },
            { "Q7", "q7" },
            { "3 Series", "3xi" },
            { "5 Series", "5xi" },
            { "X3", "x3" },
            { "X5", "x5" },
            { "C-Class", "cji" },
            { "E-Class", "eji" },
            { "Golf", "gaoerfu" },
            { "Passat", "pasate" },
            { "Camry", "kaimrui" },
            { "Corolla", "kaluola" },
            { "CR-V", "crv" }
        };

        return modelSlugs.TryGetValue(modelName, out var slug) ? slug : modelName.ToLowerInvariant().Replace(" ", "-");
    }
}

/// <summary>
/// Расширение для регистрации Worker
/// </summary>
public static class CarSearchQueueWorkerExtensions
{
    public static IServiceCollection AddCarSearchQueueWorker(this IServiceCollection services)
    {
        services.AddHostedService<CarSearchQueueWorker>();
        return services;
    }
}
