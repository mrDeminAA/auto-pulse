using AutoPulse.Domain;
using AutoPulse.Application.Parsing;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace AutoPulse.Infrastructure.Services;

/// <summary>
/// Сервис обработки спарсенных данных
/// </summary>
public interface IParsedDataService
{
    Task<Car> ProcessParsedCarAsync(ParsedCarData parsedData, CancellationToken cancellationToken = default);
}

public class ParsedDataService : IParsedDataService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ParsedDataService> _logger;

    public ParsedDataService(ApplicationDbContext dbContext, ILogger<ParsedDataService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Car> ProcessParsedCarAsync(ParsedCarData parsedData, CancellationToken cancellationToken = default)
    {
        // 1. Находим или создаём марку
        var brand = await GetOrCreateBrandAsync(parsedData.BrandName, parsedData.Currency == "CNY" ? "China" : null, cancellationToken);

        // 2. Находим или создаём модель
        var model = await GetOrCreateModelAsync(brand.Id, parsedData.ModelName, cancellationToken);

        // 3. Находим рынок (Китай)
        var market = await GetOrCreateMarketAsync("China", "Asia", "CNY", cancellationToken);

        // 4. Проверяем, есть ли уже такой автомобиль по URL
        var existingCar = await _dbContext.Cars
            .FirstOrDefaultAsync(c => c.SourceUrl == parsedData.SourceUrl, cancellationToken);

        if (existingCar != null)
        {
            // Обновляем цену и статус
            existingCar.UpdatePrice(parsedData.Price);
            if (!parsedData.SourceUrl.Contains("sold", StringComparison.OrdinalIgnoreCase))
            {
                existingCar.SetDataSource(1); // TODO: получить ID источника
            }
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Обновлён автомобиль {Url}", parsedData.SourceUrl);
            return existingCar;
        }

        // 5. Создаём новый автомобиль
        var car = new Car(
            brand.Id,
            model.Id,
            market.Id,
            parsedData.Year,
            parsedData.Price,
            parsedData.Currency,
            parsedData.SourceUrl
        );

        car.SetDetails(
            parsedData.Mileage,
            parsedData.Transmission,
            parsedData.Engine,
            parsedData.FuelType,
            parsedData.Color
        );

        car.SetVin(parsedData.Vin);
        car.SetLocation(parsedData.Location, "China");
        car.SetImageUrl(parsedData.ImageUrl);

        _dbContext.Cars.Add(car);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Создан автомобиль {Brand} {Model} {Year}", 
            parsedData.BrandName, parsedData.ModelName, parsedData.Year);

        return car;
    }

    private async Task<Brand> GetOrCreateBrandAsync(string name, string? country, CancellationToken ct)
    {
        var brand = await _dbContext.Brands
            .FirstOrDefaultAsync(b => b.Name == name, ct);

        if (brand == null)
        {
            brand = new Brand(name, country);
            _dbContext.Brands.Add(brand);
            await _dbContext.SaveChangesAsync(ct);
        }

        return brand;
    }

    private async Task<Model> GetOrCreateModelAsync(int brandId, string name, CancellationToken ct)
    {
        var model = await _dbContext.Models
            .FirstOrDefaultAsync(m => m.BrandId == brandId && m.Name == name, ct);

        if (model == null)
        {
            model = new Model(brandId, name, null);
            _dbContext.Models.Add(model);
            await _dbContext.SaveChangesAsync(ct);
        }

        return model;
    }

    private async Task<Market> GetOrCreateMarketAsync(string name, string region, string currency, CancellationToken ct)
    {
        var market = await _dbContext.Markets
            .FirstOrDefaultAsync(m => m.Name == name, ct);

        if (market == null)
        {
            market = new Market(name, region, currency);
            _dbContext.Markets.Add(market);
            await _dbContext.SaveChangesAsync(ct);
        }

        return market;
    }
}
