using AutoPulse.Parsing;
using AutoPulse.Domain;
using AutoPulse.Infrastructure;
using AutoPulse.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Worker;

/// <summary>
/// Сервис для сохранения автомобилей в БД
/// </summary>
public interface ICarStorageService
{
    Task SaveAsync(ParsedCarData car, CancellationToken cancellationToken = default);
    Task SaveBatchAsync(IEnumerable<ParsedCarData> cars, CancellationToken cancellationToken = default);
    Task<int> SaveCarsForQueueAsync(List<ParsedCarData> cars, int queueId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Реализация сервиса хранения с конвертацией валют
/// </summary>
public class CarStorageService : ICarStorageService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrencyConversionService _currencyService;
    private readonly ILogger<CarStorageService> _logger;
    private const string TargetCurrency = "RUB";

    public CarStorageService(
        ApplicationDbContext dbContext,
        ICurrencyConversionService currencyService,
        ILogger<CarStorageService> logger)
    {
        _dbContext = dbContext;
        _currencyService = currencyService;
        _logger = logger;
    }

    public async Task SaveAsync(ParsedCarData car, CancellationToken cancellationToken = default)
    {
        // Определяем валюту по источнику
        var sourceCurrency = GetCurrencyForSource(car.Source);
        
        // Конвертируем цену в RUB
        var priceRub = await _currencyService.ConvertAsync(car.Price, sourceCurrency, TargetCurrency, cancellationToken);

        _logger.LogInformation("Сохранение автомобиля: {Brand} {Model} ({Year}), Цена: {Price} {Currency} ({PriceRub} ₽)",
            car.Brand, car.Model, car.Year, car.Price, sourceCurrency, priceRub);

        // Проверяем дубликаты по URL
        var existing = await _dbContext.Cars
            .FirstOrDefaultAsync(c => c.SourceUrl == car.SourceUrl, cancellationToken);

        if (existing != null)
        {
            _logger.LogDebug("Автомобиль уже существует: {Url}", car.SourceUrl);

            // Обновляем цену если изменилась
            if (existing.Price != car.Price || existing.Currency != sourceCurrency)
            {
                existing.UpdatePrice(car.Price, sourceCurrency, priceRub);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Обновлена цена автомобиля {Id}: {OldPrice} -> {NewPrice}",
                    existing.Id, existing.Price, car.Price);
            }
            return;
        }

        // Создаём новую запись
        var newCar = CreateCarFromParsedData(car, priceRub, sourceCurrency);
        _dbContext.Cars.Add(newCar);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Сохранён новый автомобиль: {Id}, {Brand} {Model}", newCar.Id, newCar.Brand.Name, newCar.Model.Name);
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

    public async Task<int> SaveCarsForQueueAsync(List<ParsedCarData> cars, int queueId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Сохранение {Count} автомобилей для очереди {QueueId}", cars.Count, queueId);

        var savedCount = 0;

        foreach (var car in cars)
        {
            try
            {
                var sourceCurrency = GetCurrencyForSource(car.Source);
                var rate = await _currencyService.GetRateAsync(sourceCurrency, TargetCurrency, cancellationToken);
                var priceRub = car.Price * rate;

                // Проверяем дубликаты
                var exists = await _dbContext.Cars
                    .AnyAsync(c => c.SourceUrl == car.SourceUrl, cancellationToken);

                if (exists)
                {
                    _logger.LogDebug("Пропущен дубликат: {Url}", car.SourceUrl);
                    continue;
                }

                var newCar = CreateCarFromParsedData(car, priceRub, sourceCurrency);
                _dbContext.Cars.Add(newCar);
                savedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении автомобиля {Brand} {Model}", car.Brand, car.Model);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Сохранено {Count} из {Total} автомобилей", savedCount, cars.Count);

        return savedCount;
    }

    private Car CreateCarFromParsedData(ParsedCarData parsed, decimal priceRub, string sourceCurrency)
    {
        // Получаем или создаём бренд
        var brand = _dbContext.Brands
            .FirstOrDefault(b => b.Name == parsed.Brand)
            ?? new Brand(parsed.Brand);

        // Получаем или создаём модель
        var model = _dbContext.Models
            .FirstOrDefault(m => m.Name == parsed.Model && m.BrandId == brand.Id)
            ?? new Model(brand.Id, parsed.Model);

        // Определяем регион и рынок по источнику
        var (region, regionName, marketCurrency) = GetMarketInfoForSource(parsed.Source);

        // Получаем или создаём рынок
        var market = _dbContext.Markets
            .FirstOrDefault(m => m.Region == region)
            ?? new Market(region, regionName, marketCurrency);

        // Получаем или создаём источник данных
        var dataSource = GetOrCreateDataSource(parsed.Source, region);

        var car = new Car(
            brand.Id,
            model.Id,
            market.Id,
            parsed.Year,
            parsed.Price,
            sourceCurrency,
            priceRub,
            parsed.SourceUrl,
            dataSource.Id
        );

        // Дополнительные данные
        if (parsed.Mileage > 0)
            car.UpdateMileage(parsed.Mileage);

        if (!string.IsNullOrEmpty(parsed.City))
            car.UpdateLocation(parsed.City, regionName);

        if (!string.IsNullOrEmpty(parsed.ImageUrl))
            car.UpdateImageUrl(parsed.ImageUrl);

        return car;
    }

    private (string region, string regionName, string currency) GetMarketInfoForSource(string source)
    {
        return source.ToLowerInvariant() switch
        {
            "che168" => ("China", "Китай", "CNY"),
            "mobile.de" or "mobilede" => ("Europe", "Европа", "EUR"),
            "cars.com" or "carscom" => ("USA", "США", "USD"),
            _ => ("China", "Китай", "CNY")
        };
    }

    private string GetCurrencyForSource(string source)
    {
        return source.ToLowerInvariant() switch
        {
            "che168" => "CNY",
            "mobile.de" or "mobilede" => "EUR",
            "cars.com" or "carscom" => "USD",
            _ => "CNY"
        };
    }

    private DataSource GetOrCreateDataSource(string sourceName, string region)
    {
        var dataSource = _dbContext.DataSources
            .FirstOrDefault(d => d.Name == sourceName);

        if (dataSource == null)
        {
            var url = sourceName.ToLowerInvariant() switch
            {
                "che168" => "https://m.che168.com",
                "mobile.de" or "mobilede" => "https://www.mobile.de",
                "cars.com" or "carscom" => "https://www.cars.com",
                _ => "https://www.che168.com"
            };

            dataSource = new DataSource(sourceName, region, url);
            _dbContext.DataSources.Add(dataSource);
        }

        return dataSource;
    }
}
