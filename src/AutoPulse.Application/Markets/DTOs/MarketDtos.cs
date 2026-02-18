namespace AutoPulse.Application.Markets.DTOs;

/// <summary>
/// DTO для представления рынка
/// </summary>
public record MarketDto(
    int Id,
    string Name,
    string Region,
    string Currency,
    DateTime CreatedAt
);

/// <summary>
/// DTO для представления рынка с детальной информацией
/// </summary>
public record MarketDetailsDto(
    int Id,
    string Name,
    string Region,
    string Currency,
    DateTime CreatedAt,
    int DealersCount,
    int CarsCount
);

/// <summary>
/// DTO для статистики автомобилей на рынке
/// </summary>
public record CarsCountDto(
    int TotalCount,
    int AvailableCount
);
