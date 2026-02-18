using Microsoft.EntityFrameworkCore;
using AutoPulse.Domain;
using AutoPulse.Infrastructure;

namespace AutoPulse.Api.Endpoints;

/// <summary>
/// API для работы с автомобилями
/// </summary>
public static class CarsEndpoints
{
    public static void MapCarsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cars")
            .WithTags("Cars");

        // GET /api/cars
        group.MapGet("/", async (
            ApplicationDbContext db,
            int? brandId = null,
            int? modelId = null,
            int? marketId = null,
            int? minYear = null,
            int? maxYear = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var query = db.Cars
                .AsNoTracking()
                .Include(c => c.Brand)
                .Include(c => c.Model)
                .Include(c => c.Market)
                .Include(c => c.Dealer)
                .AsQueryable();

            // Фильтры
            if (brandId.HasValue)
                query = query.Where(c => c.BrandId == brandId.Value);

            if (modelId.HasValue)
                query = query.Where(c => c.ModelId == modelId.Value);

            if (marketId.HasValue)
                query = query.Where(c => c.MarketId == marketId.Value);

            if (minYear.HasValue)
                query = query.Where(c => c.Year >= minYear.Value);

            if (maxYear.HasValue)
                query = query.Where(c => c.Year <= maxYear.Value);

            if (minPrice.HasValue)
                query = query.Where(c => c.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(c => c.Price <= maxPrice.Value);

            // Пагинация
            var totalCount = await query.CountAsync(ct);
            var cars = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CarDto(
                    c.Id,
                    c.BrandId,
                    c.ModelId,
                    c.MarketId,
                    c.DealerId,
                    c.DataSourceId,
                    c.Year,
                    c.Price,
                    c.Currency,
                    c.Vin,
                    c.Mileage,
                    c.Transmission,
                    c.Engine,
                    c.FuelType,
                    c.Color,
                    c.Location,
                    c.Country,
                    c.SourceUrl,
                    c.ImageUrl,
                    c.IsAvailable,
                    c.CreatedAt,
                    c.Brand.Name,
                    c.Model.Name,
                    c.Market.Name,
                    c.Dealer != null ? c.Dealer.Name : null
                ))
                .ToListAsync(ct);

            return Results.Ok(new PagedResult<CarDto>(
                cars,
                totalCount,
                page,
                pageSize
            ));
        })
        .WithName("GetAllCars")
        .WithSummary("Получить список автомобилей с фильтрацией и пагинацией")
        .Produces<PagedResult<CarDto>>(StatusCodes.Status200OK);

        // GET /api/cars/{id}
        group.MapGet("/{id:int}", async (int id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var car = await db.Cars
                .AsNoTracking()
                .Include(c => c.Brand)
                .Include(c => c.Model)
                .Include(c => c.Market)
                .Include(c => c.Dealer)
                .Include(c => c.DataSource)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (car == null)
                return Results.NotFound();

            return Results.Ok(new CarDetailsDto(
                car.Id,
                car.BrandId,
                car.ModelId,
                car.MarketId,
                car.DealerId,
                car.DataSourceId,
                car.Year,
                car.Price,
                car.Currency,
                car.Vin,
                car.Mileage,
                car.Transmission,
                car.Engine,
                car.FuelType,
                car.Color,
                car.Location,
                car.Country,
                car.SourceUrl,
                car.ImageUrl,
                car.IsAvailable,
                car.CreatedAt,
                car.UpdatedAt,
                car.SoldAt,
                car.Brand.Name,
                car.Model.Name,
                car.Market.Name,
                car.Market.Region,
                car.Dealer != null ? new DealerInfoDto(car.Dealer.Id, car.Dealer.Name, car.Dealer.Rating, car.Dealer.ContactInfo) : null,
                car.DataSource != null ? new SourceInfoDto(car.DataSource.Id, car.DataSource.Name, car.DataSource.Country) : null
            ));
        })
        .WithName("GetCarById")
        .WithSummary("Получить детали автомобиля по ID")
        .Produces<CarDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/cars/stats/summary
        group.MapGet("/stats/summary", async (ApplicationDbContext db, CancellationToken ct) =>
        {
            var totalCount = await db.Cars.CountAsync(ct);
            var availableCount = await db.Cars.CountAsync(c => c.IsAvailable, ct);
            
            var avgPrice = await db.Cars
                .Where(c => c.IsAvailable)
                .AverageAsync(c => c.Price);

            var byMarket = await db.Cars
                .Include(c => c.Market)
                .GroupBy(c => c.Market.Name)
                .Select(g => new MarketStatDto(g.Key, g.Count()))
                .ToListAsync(ct);

            return Results.Ok(new StatsSummaryDto(
                totalCount,
                availableCount,
                avgPrice,
                byMarket
            ));
        })
        .WithName("GetCarsStats")
        .WithSummary("Получить статистику по автомобилям")
        .Produces<StatsSummaryDto>(StatusCodes.Status200OK);
    }

    public record CarDto(
        int Id,
        int BrandId,
        int ModelId,
        int MarketId,
        int? DealerId,
        int? DataSourceId,
        int Year,
        decimal Price,
        string Currency,
        string? Vin,
        int Mileage,
        string? Transmission,
        string? Engine,
        string? FuelType,
        string? Color,
        string? Location,
        string? Country,
        string SourceUrl,
        string? ImageUrl,
        bool IsAvailable,
        DateTime CreatedAt,
        string BrandName,
        string ModelName,
        string MarketName,
        string? DealerName
    );

    public record CarDetailsDto(
        int Id,
        int BrandId,
        int ModelId,
        int MarketId,
        int? DealerId,
        int? DataSourceId,
        int Year,
        decimal Price,
        string Currency,
        string? Vin,
        int Mileage,
        string? Transmission,
        string? Engine,
        string? FuelType,
        string? Color,
        string? Location,
        string? Country,
        string SourceUrl,
        string? ImageUrl,
        bool IsAvailable,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        DateTime? SoldAt,
        string BrandName,
        string ModelName,
        string MarketName,
        string MarketRegion,
        DealerInfoDto? Dealer,
        SourceInfoDto? DataSource
    );

    public record DealerInfoDto(int Id, string Name, decimal Rating, string? ContactInfo);
    
    public record SourceInfoDto(int Id, string Name, string Country);

    public record PagedResult<T>(
        List<T> Items,
        int TotalCount,
        int Page,
        int PageSize
    )
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }

    public record StatsSummaryDto(
        int TotalCount,
        int AvailableCount,
        decimal? AveragePrice,
        List<MarketStatDto> ByMarket
    );

    public record MarketStatDto(string Market, int Count);
}
