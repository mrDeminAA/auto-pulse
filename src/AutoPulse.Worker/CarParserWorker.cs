using System.Threading.Channels;
using AutoPulse.Parsing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Worker;

/// <summary>
/// Фоновый сервис для парсинга и сохранения автомобилей
/// </summary>
public class CarParserWorker : BackgroundService
{
    private readonly Che168PlaywrightParser _parser;
    private readonly ICarParseQueue _queue;
    private readonly ICarStorageService _storage;
    private readonly ILogger<CarParserWorker> _logger;
    private readonly IConfiguration _configuration;

    public CarParserWorker(
        Che168PlaywrightParser parser,
        ICarParseQueue queue,
        ICarStorageService storage,
        ILogger<CarParserWorker> logger,
        IConfiguration configuration)
    {
        _parser = parser;
        _queue = queue;
        _storage = storage;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Car Parser Worker запущен");

        // Запускаем обработку очереди
        var processTask = ProcessQueueAsync(stoppingToken);

        // Запускаем парсинг
        var parseTask = RunParsingAsync(stoppingToken);

        await Task.WhenAll(processTask, parseTask);
    }

    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Обработчик очереди запущен");

        if (_queue is CarParseQueue queueImpl)
        {
            await foreach (var car in queueImpl.DequeueAsync(cancellationToken))
            {
                try
                {
                    await _storage.SaveAsync(car, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при сохранении автомобиля {Id}", car.Id);
                }
            }
        }
    }

    private async Task RunParsingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Парсер запущен");

        // Получаем настройки
        var brandUrl = _configuration.GetValue<string>("Che168:BrandUrl") ?? "https://www.che168.com/beijing/aodi/a3/";
        var maxPages = _configuration.GetValue<int>("Che168:MaxPages", 20);
        var targetCount = _configuration.GetValue<int>("Che168:TargetCount", 100);

        _logger.LogInformation("Парсинг URL {Url}, макс. страниц: {MaxPages}, цель: {Target} авто", brandUrl, maxPages, targetCount);

        try
        {
            await foreach (var car in _parser.ParseAllAsync(brandUrl, maxPages, targetCount, cancellationToken))
            {
                await _queue.EnqueueAsync(car, cancellationToken);
                _logger.LogDebug("Автомобиль добавлен в очередь: {Id} - {Brand} {Model}", car.Id, car.Brand, car.Model);
            }

            _logger.LogInformation("Парсинг завершен");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Парсинг отменен");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при парсинге");
        }
    }
}
