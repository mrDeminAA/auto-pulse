using AutoPulse.Domain;
using AutoPulse.Infrastructure;
using AutoPulse.Infrastructure.Services;
using AutoPulse.Parsing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

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
    private IPlaywright? _playwright;
    private IBrowser? _browser;

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

        try
        {
            // Инициализируем Playwright один раз
            await InitializeBrowserAsync(stoppingToken);

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
        finally
        {
            await CleanupAsync();
        }
    }

    private async Task InitializeBrowserAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Инициализация Playwright...");

        _playwright = await Playwright.CreateAsync();
        
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage" }
        });

        _logger.LogInformation("Playwright инициализирован");
    }

    private async Task CleanupAsync()
    {
        _logger.LogInformation("Очистка ресурсов...");
        
        if (_browser != null)
        {
            await _browser.CloseAsync();
        }
        
        _playwright?.Dispose();
        
        _logger.LogInformation("Ресурсы очищены");
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
        _logger.LogInformation("Обработка очереди {QueueId}: {Brand} {Model} ({YearFrom}-{YearTo}), Priority={Priority}",
            queue.Id, queue.Brand?.Name, queue.Model?.Name, queue.YearFrom, queue.YearTo, queue.Priority);

        // Блокируем задачу (чтобы другие воркеры не обрабатывали)
        queue.StartParsing();
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            // Создаём парсер
            var parserLogger = _serviceProvider.GetRequiredService<ILogger<Che168PlaywrightParser>>();
            var parser = new Che168PlaywrightParser(_browser!, parserLogger);

            // Формируем URL для парсинга
            var brandSlug = GetBrandSlug(queue.Brand?.Name);
            var modelSlug = GetModelSlug(queue.Model?.Name);
            
            if (string.IsNullOrEmpty(brandSlug) || string.IsNullOrEmpty(modelSlug))
            {
                _logger.LogWarning("Не удалось определить slug для бренда/модели");
                queue.FailParsing("Неверные параметры бренда или модели");
                await dbContext.SaveChangesAsync(cancellationToken);
                return;
            }

            var baseUrl = $"https://m.che168.com/carlist/{brandSlug}/{modelSlug}/";
            
            // Парсим автомобили
            var cars = new List<ParsedCarData>();
            await foreach (var car in parser.ParseAllAsync(baseUrl, 10, 50, cancellationToken))
            {
                // Фильтруем по году
                if (car.Year >= queue.YearFrom && car.Year <= queue.YearTo)
                {
                    cars.Add(car);
                }

                if (cars.Count >= 100)
                    break;
            }

            _logger.LogInformation("Распаршено {Count} автомобилей для очереди {QueueId}", cars.Count, queue.Id);

            if (cars.Any())
            {
                // Сохраняем автомобили
                var storageLogger = _serviceProvider.GetRequiredService<ILogger<CarStorageService>>();
                var storageService = new CarStorageService(dbContext, currencyService, storageLogger);
                var savedCount = await storageService.SaveCarsForQueueAsync(cars, queue.Id, cancellationToken);

                _logger.LogInformation("Сохранено {Count} автомобилей", savedCount);
            }

            // Завершаем успешно
            queue.CompleteParsing(cars.Count);
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
