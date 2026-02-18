using AutoPulse.Parsing;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Worker;

/// <summary>
/// Сервис для сохранения автомобилей в БД
/// </summary>
public interface ICarStorageService
{
    Task SaveAsync(ParsedCarData car, CancellationToken cancellationToken = default);
    Task SaveBatchAsync(IEnumerable<ParsedCarData> cars, CancellationToken cancellationToken = default);
}

/// <summary>
/// Реализация сервиса хранения (заглушка для примера)
/// </summary>
public class CarStorageService : ICarStorageService
{
    private readonly ILogger<CarStorageService> _logger;

    public CarStorageService(ILogger<CarStorageService> logger)
    {
        _logger = logger;
    }

    public Task SaveAsync(ParsedCarData car, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Сохранение автомобиля: {Brand} {Model} ({Year}), Цена: {Price} ¥",
            car.Brand, car.Model, car.Year, car.Price);

        // TODO: Здесь будет реальная запись в БД
        // Например: await _dbContext.Cars.AddAsync(car.ToEntity(), cancellationToken);
        // await _dbContext.SaveChangesAsync(cancellationToken);

        return Task.CompletedTask;
    }

    public async Task SaveBatchAsync(IEnumerable<ParsedCarData> cars, CancellationToken cancellationToken = default)
    {
        var carsList = cars.ToList();
        _logger.LogInformation("Пакетное сохранение {Count} автомобилей", carsList.Count);

        foreach (var car in carsList)
        {
            await SaveAsync(car, cancellationToken);
        }

        _logger.LogInformation("Пакетное сохранение завершено");
    }
}
