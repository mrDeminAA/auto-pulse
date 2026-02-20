using AutoPulse.Domain;
using AutoPulse.Infrastructure;
using AutoPulse.Api.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutoPulse.Api.Endpoints;

/// <summary>
/// API для управления поиском автомобилей пользователями
/// </summary>
public static class UserSearchEndpoints
{
    public static void MapUserSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/user/search")
            .WithTags("UserSearch")
            .RequireAuthorization();

        // GET /api/user/search - получить текущий поиск пользователя
        group.MapGet("/", async (
            HttpContext httpContext,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            var userId = httpContext.User.GetCurrentUserId();
            if (userId == null)
                return Results.Unauthorized();

            var userSearch = await db.UserSearchs
                .Include(us => us.Brand)
                .Include(us => us.Model)
                .FirstOrDefaultAsync(us => us.UserId == userId.Value, ct);

            if (userSearch == null)
                return Results.NotFound();

            return Results.Ok(new UserSearchResponse(
                userSearch.Id,
                userSearch.UserId,
                userSearch.BrandId,
                userSearch.ModelId,
                userSearch.Brand?.Name,
                userSearch.Model?.Name,
                userSearch.Generation,
                userSearch.YearFrom,
                userSearch.YearTo,
                userSearch.MaxPrice,
                userSearch.MaxMileage,
                userSearch.Regions,
                userSearch.Status.ToString(),
                userSearch.CreatedAt
            ));
        })
        .WithName("GetUserSearch")
        .WithSummary("Получить текущий поиск пользователя")
        .Produces<UserSearchResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/user/search - создать или обновить поиск
        group.MapPost("/", async (
            HttpContext httpContext,
            UserSearchRequest request,
            ApplicationDbContext db,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("UserSearch");
            var userId = httpContext.User.GetCurrentUserId();
            if (userId == null)
                return Results.Unauthorized();

            // Проверяем есть ли уже поиск у пользователя
            var existingSearch = await db.UserSearchs
                .FirstOrDefaultAsync(us => us.UserId == userId.Value, ct);

            if (existingSearch != null)
            {
                // Обновляем существующий
                existingSearch.SetParameters(
                    request.BrandId,
                    request.ModelId,
                    request.Generation,
                    request.YearFrom,
                    request.YearTo,
                    request.MaxPrice,
                    request.MaxMileage,
                    request.Regions
                );

                existingSearch.SetStatus(SearchStatus.Active);
                existingSearch.UpdateTimestamp();

                logger.LogInformation("Обновлён поиск пользователя {UserId}: Brand={BrandId}, Model={ModelId}",
                    userId, request.BrandId, request.ModelId);
            }
            else
            {
                // Создаём новый
                var userSearch = new UserSearch(userId.Value, request.BrandId, request.ModelId);
                userSearch.SetParameters(
                    null,
                    null,
                    request.Generation,
                    request.YearFrom,
                    request.YearTo,
                    request.MaxPrice,
                    request.MaxMileage,
                    request.Regions
                );
                db.UserSearchs.Add(userSearch);

                logger.LogInformation("Создан новый поиск пользователя {UserId}: Brand={BrandId}, Model={ModelId}",
                    userId, request.BrandId, request.ModelId);
            }

            await db.SaveChangesAsync(ct);

            // Добавляем в очередь на парсинг (передаем UserSearch.Id)
            var userSearchId = existingSearch?.Id ?? db.UserSearchs
                .FirstOrDefault(us => us.UserId == userId.Value)!.Id;
            await AddToQueueAsync(db, userSearchId, request, ct);

            return Results.Ok(new { message = "Поиск сохранён, парсинг запущен" });
        })
        .WithName("SaveUserSearch")
        .WithSummary("Сохранить поиск пользователя и запустить парсинг")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        // DELETE /api/user/search - удалить поиск
        group.MapDelete("/", async (
            HttpContext httpContext,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            var userId = httpContext.User.GetCurrentUserId();
            if (userId == null)
                return Results.Unauthorized();

            var userSearch = await db.UserSearchs
                .FirstOrDefaultAsync(us => us.UserId == userId.Value, ct);

            if (userSearch == null)
                return Results.NotFound();

            userSearch.SetStatus(SearchStatus.Cancelled);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { message = "Поиск удалён" });
        })
        .WithName("DeleteUserSearch")
        .WithSummary("Удалить поиск пользователя")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/user/search/status - получить статус парсинга
        group.MapGet("/status", async (
            HttpContext httpContext,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            var userId = httpContext.User.GetCurrentUserId();
            if (userId == null)
                return Results.Unauthorized();

            var userSearch = await db.UserSearchs
                .Include(us => us.SearchQueues)
                    .ThenInclude(q => q.CarSearchQueue)
                .FirstOrDefaultAsync(us => us.UserId == userId.Value, ct);

            if (userSearch == null)
                return Results.NotFound();

            var queueStatuses = userSearch.SearchQueues
                .Select(q => new
                {
                    q.CarSearchQueue.Id,
                    q.CarSearchQueue.Status,
                    q.CarSearchQueue.LastParsedAt,
                    q.CarSearchQueue.NextParseAt,
                    q.CarSearchQueue.Priority
                })
                .ToList();

            return Results.Ok(new
            {
                userSearch.Id,
                userSearch.Status,
                Queues = queueStatuses
            });
        })
        .WithName("GetUserSearchStatus")
        .WithSummary("Получить статус парсинга")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task AddToQueueAsync(
        ApplicationDbContext db,
        int userSearchId,
        UserSearchRequest request,
        CancellationToken ct)
    {
        // Проверяем есть ли уже такая задача в очереди
        var existingQueue = await db.CarSearchQueues
            .FirstOrDefaultAsync(q =>
                q.BrandId == request.BrandId &&
                q.ModelId == request.ModelId &&
                q.Generation == request.Generation &&
                q.YearFrom == request.YearFrom &&
                q.YearTo == request.YearTo,
                ct);

        if (existingQueue != null)
        {
            // Уже есть такая задача - увеличиваем приоритет
            existingQueue.IncrementPriority();

            // Проверяем есть ли связь с этим поиском
            var existingLink = await db.UserSearchQueues
                .FirstOrDefaultAsync(l =>
                    l.UserSearchId == userSearchId &&
                    l.CarSearchQueueId == existingQueue.Id, ct);

            if (existingLink == null)
            {
                db.UserSearchQueues.Add(new UserSearchQueue(userSearchId, existingQueue.Id));
            }
        }
        else
        {
            // Создаём новую задачу
            var regions = request.Regions ?? "[\"china\"]";
            var newQueue = new CarSearchQueue(
                request.YearFrom ?? 2015,
                request.YearTo ?? DateTime.UtcNow.Year,
                regions,
                request.BrandId,
                request.ModelId,
                request.Generation
            );
            db.CarSearchQueues.Add(newQueue);

            // Сохраняем чтобы получить ID
            await db.SaveChangesAsync(ct);

            // Создаём связь
            db.UserSearchQueues.Add(new UserSearchQueue(userSearchId, newQueue.Id));
        }

        await db.SaveChangesAsync(ct);
    }

    private static int? GetCurrentUserId(this System.Security.Claims.ClaimsPrincipal user)
    {
        var claim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var userId))
            return null;
        
        return userId;
    }
}
