using System.Threading.Channels;
using AutoPulse.Parsing;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Worker;

/// <summary>
/// Сервис очереди для обработки спарсенных автомобилей
/// </summary>
public interface ICarParseQueue
{
    /// <summary>
    /// Добавить автомобиль в очередь
    /// </summary>
    Task EnqueueAsync(ParsedCarData car, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить количество элементов в очереди
    /// </summary>
    int QueueLength { get; }
}

/// <summary>
/// Реализация очереди на основе Channel
/// </summary>
public class CarParseQueue : ICarParseQueue
{
    private readonly Channel<ParsedCarData> _channel;
    private readonly ILogger<CarParseQueue> _logger;

    public CarParseQueue(ILogger<CarParseQueue> logger)
    {
        _logger = logger;
        // Создаем канал с ограничением в 1000 элементов
        _channel = Channel.CreateBounded<ParsedCarData>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public int QueueLength => _channel.Reader.Count;

    public async Task EnqueueAsync(ParsedCarData car, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(car, cancellationToken);
        _logger.LogDebug("Автомобиль добавлен в очередь: {Brand} {Model}, ID={Id}",
            car.Brand, car.Model, car.Id);
    }

    public IAsyncEnumerable<ParsedCarData> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
