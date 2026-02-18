using MediatR;
using AutoPulse.Application.Markets.Queries;
using AutoPulse.Application.Markets.DTOs;
using AutoPulse.Application.Dealers.DTOs;

namespace AutoPulse.Api.Endpoints;

/// <summary>
/// API для работы с рынками через CQRS + MediatR
/// </summary>
public static class MarketsEndpoints
{
    public static void MapMarketsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/markets")
            .WithTags("Markets")
            .WithOpenApi();

        // GET /api/markets
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var query = new GetAllMarketsQuery();
            var markets = await sender.Send(query, ct);
            return Results.Ok(markets);
        })
        .WithName("GetAllMarkets")
        .WithSummary("Получить список всех рынков")
        .Produces<List<MarketDto>>(StatusCodes.Status200OK);

        // GET /api/markets/{id}
        group.MapGet("/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
        {
            var query = new GetMarketByIdQuery(id);
            var market = await sender.Send(query, ct);

            if (market == null)
                return Results.NotFound();

            return Results.Ok(market);
        })
        .WithName("GetMarketById")
        .WithSummary("Получить рынок по ID")
        .Produces<MarketDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/markets/{id}/dealers
        group.MapGet("/{id:int}/dealers", async (int id, ISender sender, CancellationToken ct) =>
        {
            var query = new GetMarketDealersQuery(id);
            var dealers = await sender.Send(query, ct);
            return Results.Ok(dealers);
        })
        .WithName("GetMarketDealers")
        .WithSummary("Получить дилеров рынка")
        .Produces<List<DealerBriefDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/markets/{id}/cars/count
        group.MapGet("/{id:int}/cars/count", async (int id, ISender sender, CancellationToken ct) =>
        {
            var query = new GetMarketCarsCountQuery(id);
            var result = await sender.Send(query, ct);

            if (result == null)
                return Results.NotFound();

            return Results.Ok(result);
        })
        .WithName("GetMarketCarsCount")
        .WithSummary("Получить количество автомобилей на рынке")
        .Produces<CarsCountDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
