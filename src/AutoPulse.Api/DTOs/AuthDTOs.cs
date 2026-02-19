namespace AutoPulse.Api.DTOs;

/// <summary>
/// Регистрация пользователя
/// </summary>
public record RegisterRequest(
    string Email,
    string Password,
    string? Name = null
);

/// <summary>
/// Вход пользователя
/// </summary>
public record LoginRequest(
    string Email,
    string Password
);

/// <summary>
/// Ответ с токеном
/// </summary>
public record AuthResponse(
    int UserId,
    string Email,
    string Token,
    DateTime ExpiresAt
);

/// <summary>
/// Информация о пользователе
/// </summary>
public record UserDto(
    int Id,
    string Email,
    string? Name,
    string? AvatarUrl,
    string? TelegramId,
    DateTime CreatedAt
);

/// <summary>
/// Параметры поиска пользователя
/// </summary>
public record UserSearchRequest(
    int? BrandId = null,
    int? ModelId = null,
    string? Generation = null,
    int? YearFrom = null,
    int? YearTo = null,
    decimal? MaxPrice = null,
    int? MaxMileage = null,
    string? Regions = null
);

/// <summary>
/// Результат поиска
/// </summary>
public record UserSearchResponse(
    int Id,
    int UserId,
    int? BrandId,
    int? ModelId,
    string? BrandName,
    string? ModelName,
    string? Generation,
    int? YearFrom,
    int? YearTo,
    decimal? MaxPrice,
    int? MaxMileage,
    string? Regions,
    string Status,
    DateTime CreatedAt
);

/// <summary>
/// Обновление профиля
/// </summary>
public record UpdateProfileRequest(
    string? Name = null,
    string? AvatarUrl = null,
    string? TelegramId = null
);
