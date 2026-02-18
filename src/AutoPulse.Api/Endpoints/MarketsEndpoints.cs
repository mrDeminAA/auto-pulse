using Microsoft.EntityFrameworkCore;
using AutoPulse.Domain;
using AutoPulse.Infrastructure;

namespace AutoPulse.Api.Endpoints;

/// <summary>
/// API для работы с рынками (регионами)
/// </summary>
public static class MarketsEndpoints
{
    public static void MapMarketsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/markets")
            .WithTags("Markets")
            .WithOpenApi();

        // GET /api/markets
        group.MapGet("/", async (ApplicationDbContext db, CancellationToken ct) =>
        {
            var markets = await db.Markets
                .AsNoTracking()
                .OrderBy(m => m.Name)
                .Select(m => new MarketDto(
                    m.Id,
                    m.Name,
                    m.Region,
                    m.Currency,
                    m.CreatedAt
                ))
                .ToListAsync(ct);

            return Results.Ok(markets);
        })
        .WithName("GetAllMarkets")
        .WithSummary("Получить список всех рынков")
        .Produces<List<MarketDto>>(StatusCodes.Status200OK);

        // GET /api/markets/{id}
        group.MapGet("/{id:int}", async (int id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var market = await db.Markets
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id, ct);

            if (market == null)
                return Results.NotFound();

            return Results.Ok(new MarketDetailsDto(
                market.Id,
                market.Name,
                market.Region,
                market.Currency,
                market.CreatedAt
            ));
        })
        .WithName("GetMarketById")
        .WithSummary("Получить рынок по ID")
        .Produces<MarketDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/markets/{id}/dealers
        group.MapGet("/{id:int}/dealers", async (int id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var market = await db.Markets
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id, ct);

            if (market == null)
                return Results.NotFound();

            var dealers = await db.Dealers
                .AsNoTracking()
                .Where(d => d.MarketId == id)
                .OrderBy(d => d.Name)
                .Select(d => new DealerBriefDto(
                    d.Id,
                    d.Name,
                    d.Rating,
                    d.ContactInfo,
                    d.Address
                ))
                .ToListAsync(ct);

            return Results.Ok(dealers);
        })
        .WithName("GetMarketDealers")
        .WithSummary("Получить дилеров рынка")
        .Produces<List<DealerBriefDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/markets/{id}/cars/count
        group.MapGet("/{id:int}/cars/count", async (int id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var market = await db.Markets
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id, ct);

            if (market == null)
                return Results.NotFound();

            var totalCount = await db.Cars.CountAsync(c => c.MarketId == id, ct);
            var availableCount = await db.Cars.CountAsync(c => c.MarketId == id && c.IsAvailable, ct);

            return Results.Ok(new CarsCountDto(totalCount, availableCount));
        })
        .WithName("GetMarketCarsCount")
        .WithSummary("Получить количество автомобилей на рынке")
        .Produces<CarsCountDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }

    public record MarketDto(
        int Id,
        string Name,
        string Region,
        string Currency,
        DateTime CreatedAt
    );

    public record MarketDetailsDto(
        int Id,
        string Name,
        string Region,
        string Currency,
        DateTime CreatedAt
    );

    public record DealerBriefDto(
        int Id,
        string Name,
        decimal Rating,
        string? ContactInfo,
        string? Address
    );

    public record CarsCountDto(
        int TotalCount,
        int AvailableCount
    );
}
