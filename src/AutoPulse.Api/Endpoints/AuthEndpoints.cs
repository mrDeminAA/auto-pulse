using AutoPulse.Domain;
using AutoPulse.Infrastructure;
using AutoPulse.Api.DTOs;
using AutoPulse.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoPulse.Api.Endpoints;

/// <summary>
/// API для авторизации и регистрации пользователей
/// </summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        // POST /api/auth/register
        group.MapPost("/register", async (
            RegisterRequest request,
            ApplicationDbContext db,
            IJwtTokenService jwtTokenService,
            IPasswordHasher passwordHasher,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("Auth");
            
            // Проверка на существующего пользователя
            if (await db.Users.AnyAsync(u => u.Email == request.Email, ct))
            {
                logger.LogWarning("Попытка регистрации с существующим email: {Email}", request.Email);
                return Results.BadRequest(new { message = "Пользователь с таким email уже существует" });
            }

            // Хешируем пароль
            var passwordHash = passwordHasher.Hash(request.Password);

            // Создаём пользователя
            var user = new User(request.Email, passwordHash, request.Name);
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);

            logger.LogInformation("Зарегистрирован новый пользователь: {Email} (ID: {UserId})", user.Email, user.Id);

            // Генерируем токен
            var token = jwtTokenService.GenerateToken(user.Id, user.Email);

            return Results.Ok(new AuthResponse(
                user.Id,
                user.Email,
                token,
                DateTime.UtcNow.AddMinutes(60)
            ));
        })
        .WithName("RegisterUser")
        .WithSummary("Регистрация нового пользователя")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // POST /api/auth/login
        group.MapPost("/login", async (
            LoginRequest request,
            ApplicationDbContext db,
            IJwtTokenService jwtTokenService,
            IPasswordHasher passwordHasher,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("Auth");
            
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, ct);

            if (user == null)
            {
                logger.LogWarning("Попытка входа с несуществующим email: {Email}", request.Email);
                return Results.Unauthorized();
            }

            if (!user.IsEnabled)
            {
                logger.LogWarning("Попытка входа заблокированного пользователя: {Email}", request.Email);
                return Results.Unauthorized();
            }

            if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                logger.LogWarning("Неверный пароль для пользователя: {Email}", request.Email);
                return Results.Unauthorized();
            }

            // Обновляем время последнего входа
            user.MarkAsLoggedIn();
            await db.SaveChangesAsync(ct);

            logger.LogInformation("Пользователь вошёл в систему: {Email} (ID: {UserId})", user.Email, user.Id);

            // Генерируем токен
            var token = jwtTokenService.GenerateToken(user.Id, user.Email);

            return Results.Ok(new AuthResponse(
                user.Id,
                user.Email,
                token,
                DateTime.UtcNow.AddMinutes(60)
            ));
        })
        .WithName("LoginUser")
        .WithSummary("Вход пользователя")
        .Produces<AuthResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        // GET /api/auth/me - получить текущий профиль
        group.MapGet("/me", async (
            HttpContext httpContext,
            ApplicationDbContext db,
            CancellationToken ct) =>
        {
            var userId = httpContext.User.GetCurrentUserId();
            if (userId == null)
                return Results.Unauthorized();

            var user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId.Value, ct);

            if (user == null)
                return Results.NotFound();

            return Results.Ok(new UserDto(
                user.Id,
                user.Email,
                user.Name,
                user.AvatarUrl,
                user.TelegramId,
                user.CreatedAt
            ));
        })
        .WithName("GetCurrentUser")
        .WithSummary("Получить текущий профиль пользователя")
        .Produces<UserDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        // PUT /api/auth/me - обновить профиль
        group.MapPut("/me", async (
            HttpContext httpContext,
            UpdateProfileRequest request,
            ApplicationDbContext db,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("Auth");
            
            var userId = httpContext.User.GetCurrentUserId();
            if (userId == null)
                return Results.Unauthorized();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId.Value, ct);
            if (user == null)
                return Results.NotFound();

            user.UpdateProfile(request.Name, request.AvatarUrl);
            
            if (request.TelegramId != null)
                user.SetTelegramId(request.TelegramId);

            await db.SaveChangesAsync(ct);

            logger.LogInformation("Профиль пользователя обновлён: {UserId}", user.Id);

            return Results.Ok(new UserDto(
                user.Id,
                user.Email,
                user.Name,
                user.AvatarUrl,
                user.TelegramId,
                user.CreatedAt
            ));
        })
        .WithName("UpdateCurrentUser")
        .WithSummary("Обновить профиль пользователя")
        .Produces<UserDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization();
    }

    private static int? GetCurrentUserId(this System.Security.Claims.ClaimsPrincipal user)
    {
        var claim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var userId))
            return null;
        
        return userId;
    }
}
