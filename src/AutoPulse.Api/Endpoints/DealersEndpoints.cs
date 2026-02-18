using MediatR;
using AutoPulse.Application.Dealers.Queries;
using AutoPulse.Application.Dealers.DTOs;

namespace AutoPulse.Api.Endpoints;

/// <summary>
/// API для работы с дилерами через CQRS + MediatR
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
            ISender sender,
            int? marketId = null,
            decimal? minRating = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var query = new GetAllDealersQuery(marketId, minRating, page, pageSize);
            var result = await sender.Send(query, ct);
            return Results.Ok(result);
        })
        .WithName("GetAllDealers")
        .WithSummary("Получить список всех дилеров с пагинацией и фильтрами")
        .Produces<PagedResult<DealerDto>>(StatusCodes.Status200OK);

        // GET /api/dealers/{id}
        group.MapGet("/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
        {
            var query = new GetDealerByIdQuery(id);
            var dealer = await sender.Send(query, ct);

            if (dealer == null)
                return Results.NotFound();

            return Results.Ok(dealer);
        })
        .WithName("GetDealerById")
        .WithSummary("Получить дилера по ID")
        .Produces<DealerDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/dealers/market/{marketId}
        group.MapGet("/market/{marketId:int}", async (int marketId, ISender sender, CancellationToken ct) =>
        {
            var query = new GetDealersByMarketQuery(marketId);
            var dealers = await sender.Send(query, ct);
            return Results.Ok(dealers);
        })
        .WithName("GetDealersByMarket")
        .WithSummary("Получить дилеров по рынку")
        .Produces<List<DealerDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/dealers/{id}/cars
        group.MapGet("/{id:int}/cars", async (
            int id,
            ISender sender,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var query = new GetDealerCarsQuery(id, page, pageSize);
            var result = await sender.Send(query, ct);

            if (result == null)
                return Results.NotFound();

            return Results.Ok(result);
        })
        .WithName("GetDealerCars")
        .WithSummary("Получить автомобили дилера")
        .Produces<PagedResult<CarBriefDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/dealers/stats/summary
        group.MapGet("/stats/summary", async (ISender sender, CancellationToken ct) =>
        {
            var query = new GetDealerStatsQuery();
            var result = await sender.Send(query, ct);
            return Results.Ok(result);
        })
        .WithName("GetDealerStats")
        .WithSummary("Получить статистику по дилерам")
        .Produces<DealerStatsSummaryDto>(StatusCodes.Status200OK);
    }
}
