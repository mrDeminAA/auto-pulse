using MassTransit;
using AutoPulse.Infrastructure.Messaging;

namespace AutoPulse.Api.Endpoints;

/// <summary>
/// API для управления парсингом
/// </summary>
public static class ParseEndpoints
{
    public static void MapParseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/parse")
            .WithTags("Parse");

        // POST /api/parse
        group.MapPost("/", async (
            ParseRequest request,
            IPublishEndpoint publishEndpoint,
            CancellationToken ct = default) =>
        {
            var command = new ParseCarsCommand(request.Url, request.Source, request.Country);
            await publishEndpoint.Publish(command, ct);
            
            return Results.Accepted($"/api/parse/status", new 
            { 
                Message = "Команда на парсинг отправлена",
                Url = request.Url,
                Source = request.Source,
                Country = request.Country
            });
        })
        .WithName("StartParsing")
        .WithSummary("Запустить парсинг указанного URL")
        .Produces(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status400BadRequest);

        // GET /api/parse/status
        group.MapGet("/status", () => 
        {
            return Results.Ok(new { Status = "Worker is running", Queue = "parse-cars-queue" });
        })
        .WithName("GetParseStatus")
        .WithSummary("Получить статус парсинга")
        .Produces(StatusCodes.Status200OK);
    }

    public record ParseRequest(string Url, string Source, string Country);
}
