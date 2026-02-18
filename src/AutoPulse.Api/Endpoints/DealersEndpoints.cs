using Microsoft.EntityFrameworkCore;
using AutoPulse.Domain;
using AutoPulse.Infrastructure;

namespace AutoPulse.Api.Endpoints;

/// <summary>
/// API для работы с дилерами
/// </summary>
public static class DealersEndpoints
{
    public static void MapDealersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dealers")
            .WithTags("Dealers")
            .WithOpenApi();

        // GET /api/dealers
        group.MapGet("/", async (
            ApplicationDbContext db,
            int? marketId = null,
            decimal? minRating = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var query = db.Dealers
                .AsNoTracking()
                .Include(d => d.Market)
                .AsQueryable();

            // Фильтры
            if (marketId.HasValue)
                query = query.Where(d => d.MarketId == marketId.Value);

            if (minRating.HasValue)
                query = query.Where(d => d.Rating >= minRating.Value);

            // Пагинация
            var totalCount = await query.CountAsync(ct);
            var dealers = await query
                .OrderByDescending(d => d.Rating)
                .ThenBy(d => d.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DealerDto(
                    d.Id,
                    d.Name,
                    d.Rating,
                    d.ContactInfo,
                    d.Address,
                    d.MarketId,
                    d.Market.Name,
                    d.Market.Region,
                    d.CreatedAt,
                    d.UpdatedAt
                ))
                .ToListAsync(ct);

            return Results.Ok(new PagedResult<DealerDto>(
                dealers,
                totalCount,
                page,
                pageSize
            ));
        })
        .WithName("GetAllDealers")
        .WithSummary("Получить список дилеров с фильтрацией и пагинацией")
        .Produces<PagedResult<DealerDto>>(StatusCodes.Status200OK);

        // GET /api/dealers/{id}
        group.MapGet("/{id:int}", async (int id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var dealer = await db.Dealers
                .AsNoTracking()
                .Include(d => d.Market)
                .Include(d => d.Cars)
                .FirstOrDefaultAsync(d => d.Id == id, ct);

            if (dealer == null)
                return Results.NotFound();

            var carsCount = dealer.Cars.Count;
            var availableCarsCount = dealer.Cars.Count(c => c.IsAvailable);

            return Results.Ok(new DealerDetailsDto(
                dealer.Id,
                dealer.Name,
                dealer.Rating,
                dealer.ContactInfo,
                dealer.Address,
                dealer.MarketId,
                dealer.Market.Name,
                dealer.Market.Region,
                dealer.Market.Currency,
                dealer.CreatedAt,
                dealer.UpdatedAt,
                carsCount,
                availableCarsCount
            ));
        })
        .WithName("GetDealerById")
        .WithSummary("Получить детали дилера по ID")
        .Produces<DealerDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/dealers/market/{marketId}
        group.MapGet("/market/{marketId:int}", async (int marketId, ApplicationDbContext db, CancellationToken ct) =>
        {
            var market = await db.Markets
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == marketId, ct);

            if (market == null)
                return Results.NotFound();

            var dealers = await db.Dealers
                .AsNoTracking()
                .Where(d => d.MarketId == marketId)
                .OrderByDescending(d => d.Rating)
                .ThenBy(d => d.Name)
                .Select(d => new DealerDto(
                    d.Id,
                    d.Name,
                    d.Rating,
                    d.ContactInfo,
                    d.Address,
                    d.MarketId,
                    d.Market.Name,
                    d.Market.Region,
                    d.CreatedAt,
                    d.UpdatedAt
                ))
                .ToListAsync(ct);

            return Results.Ok(dealers);
        })
        .WithName("GetDealersByMarket")
        .WithSummary("Получить дилеров по рынку")
        .Produces<List<DealerDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/dealers/{id}/cars
        group.MapGet("/{id:int}/cars", async (
            int id,
            ApplicationDbContext db,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var dealer = await db.Dealers
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id, ct);

            if (dealer == null)
                return Results.NotFound();

            var query = db.Cars
                .AsNoTracking()
                .Include(c => c.Brand)
                .Include(c => c.Model)
                .Where(c => c.DealerId == id);

            var totalCount = await query.CountAsync(ct);
            var cars = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CarBriefDto(
                    c.Id,
                    c.BrandId,
                    c.ModelId,
                    c.Year,
                    c.Price,
                    c.Currency,
                    c.IsAvailable,
                    c.Brand.Name,
                    c.Model.Name
                ))
                .ToListAsync(ct);

            return Results.Ok(new PagedResult<CarBriefDto>(
                cars,
                totalCount,
                page,
                pageSize
            ));
        })
        .WithName("GetDealerCars")
        .WithSummary("Получить автомобили дилера")
        .Produces<PagedResult<CarBriefDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/dealers/stats/summary
        group.MapGet("/stats/summary", async (ApplicationDbContext db, CancellationToken ct) =>
        {
            var totalDealers = await db.Dealers.CountAsync(ct);
            
            var avgRating = await db.Dealers.AverageAsync(d => d.Rating);
            
            var topRated = await db.Dealers
                .AsNoTracking()
                .Include(d => d.Market)
                .Where(d => d.Rating >= 4.5m)
                .OrderByDescending(d => d.Rating)
                .Take(10)
                .Select(d => new DealerBriefDto(
                    d.Id,
                    d.Name,
                    d.Rating,
                    d.ContactInfo,
                    d.Address
                ))
                .ToListAsync(ct);

            var byMarket = await db.Dealers
                .Include(d => d.Market)
                .GroupBy(d => d.Market.Name)
                .Select(g => new MarketDealerStatDto(g.Key, g.Count()))
                .ToListAsync(ct);

            return Results.Ok(new DealerStatsSummaryDto(
                totalDealers,
                avgRating,
                topRated,
                byMarket
            ));
        })
        .WithName("GetDealersStats")
        .WithSummary("Получить статистику по дилерам")
        .Produces<DealerStatsSummaryDto>(StatusCodes.Status200OK);
    }

    public record DealerDto(
        int Id,
        string Name,
        decimal Rating,
        string? ContactInfo,
        string? Address,
        int MarketId,
        string MarketName,
        string MarketRegion,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );

    public record DealerDetailsDto(
        int Id,
        string Name,
        decimal Rating,
        string? ContactInfo,
        string? Address,
        int MarketId,
        string MarketName,
        string MarketRegion,
        string MarketCurrency,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        int CarsCount,
        int AvailableCarsCount
    );

    public record DealerBriefDto(
        int Id,
        string Name,
        decimal Rating,
        string? ContactInfo,
        string? Address
    );

    public record CarBriefDto(
        int Id,
        int BrandId,
        int ModelId,
        int Year,
        decimal Price,
        string Currency,
        bool IsAvailable,
        string BrandName,
        string ModelName
    );

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

    public record DealerStatsSummaryDto(
        int TotalDealers,
        decimal AverageRating,
        List<DealerBriefDto> TopRated,
        List<MarketDealerStatDto> ByMarket
    );

    public record MarketDealerStatDto(string Market, int Count);
}
