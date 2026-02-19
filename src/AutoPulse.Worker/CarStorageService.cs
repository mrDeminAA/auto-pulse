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
    private const string SourceCurrency = "CNY";

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
        // Конвертируем цену из CNY в RUB
        var priceRub = await _currencyService.ConvertAsync(car.Price, SourceCurrency, TargetCurrency, cancellationToken);
        
        _logger.LogInformation("Сохранение автомобиля: {Brand} {Model} ({Year}), Цена: {Price} ¥ ({PriceRub} ₽)",
            car.Brand, car.Model, car.Year, car.Price, priceRub);

        // Проверяем дубликаты по URL
        var existing = await _dbContext.Cars
            .FirstOrDefaultAsync(c => c.SourceUrl == car.SourceUrl, cancellationToken);
        
        if (existing != null)
        {
            _logger.LogDebug("Автомобиль уже существует: {Url}", car.SourceUrl);
            
            // Обновляем цену если изменилась
            if (existing.Price != car.Price || existing.Currency != SourceCurrency)
            {
                existing.UpdatePrice(car.Price, SourceCurrency, priceRub);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Обновлена цена автомобиля {Id}: {OldPrice} -> {NewPrice}", 
                    existing.Id, existing.Price, car.Price);
            }
            return;
        }

        // Создаём новую запись
        var newCar = CreateCarFromParsedData(car, priceRub);
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
        var currency = SourceCurrency;
        
        // Получаем курс один раз для всех автомобилей
        var rate = await _currencyService.GetRateAsync(currency, TargetCurrency, cancellationToken);
        _logger.LogInformation("Курс конвертации {From}->{To}: {Rate}", currency, TargetCurrency, rate);

        foreach (var car in cars)
        {
            try
            {
                var priceRub = car.Price * rate;
                
                // Проверяем дубликаты
                var exists = await _dbContext.Cars
                    .AnyAsync(c => c.SourceUrl == car.SourceUrl, cancellationToken);
                
                if (exists)
                {
                    _logger.LogDebug("Пропущен дубликат: {Url}", car.SourceUrl);
                    continue;
                }

                var newCar = CreateCarFromParsedData(car, priceRub);
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

    private Car CreateCarFromParsedData(ParsedCarData parsed, decimal priceRub)
    {
        // Получаем или создаём бренд
        var brand = _dbContext.Brands
            .FirstOrDefault(b => b.Name == parsed.Brand) 
            ?? new Brand(parsed.Brand);
        
        // Получаем или создаём модель
        var model = _dbContext.Models
            .FirstOrDefault(m => m.Name == parsed.Model && m.BrandId == brand.Id)
            ?? new Model(brand.Id, parsed.Model);

        // Получаем рынок (Китай)
        var market = _dbContext.Markets
            .FirstOrDefault(m => m.Region == "China")
            ?? new Market("China", "Китай", "CNY");

        // Получаем источник данных
        var dataSource = _dbContext.DataSources
            .FirstOrDefault(d => d.Name == "Che168")
            ?? new DataSource("Che168", "China", "https://m.che168.com");

        var car = new Car(
            brand.Id,
            model.Id,
            market.Id,
            parsed.Year,
            parsed.Price,
            SourceCurrency,
            priceRub,
            parsed.SourceUrl,
            dataSource.Id
        );

        // Дополнительные данные
        if (parsed.Mileage > 0)
            car.UpdateMileage(parsed.Mileage);
        
        if (!string.IsNullOrEmpty(parsed.City))
            car.UpdateLocation(parsed.City, "China");

        if (!string.IsNullOrEmpty(parsed.ImageUrl))
            car.UpdateImageUrl(parsed.ImageUrl);

        return car;
    }
}
