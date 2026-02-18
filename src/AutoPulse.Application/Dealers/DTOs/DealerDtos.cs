namespace AutoPulse.Application.Dealers.DTOs;

/// <summary>
/// DTO для представления дилера
/// </summary>
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

/// <summary>
/// DTO для детальной информации о дилере
/// </summary>
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

/// <summary>
/// DTO для представления автомобиля в контексте дилера
/// </summary>
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

/// <summary>
/// DTO для пагинированного результата
/// </summary>
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
};

/// <summary>
/// DTO для краткой информации о дилере
/// </summary>
public record DealerBriefDto(
    int Id,
    string Name,
    decimal Rating,
    string? ContactInfo,
    string? Address
);

/// <summary>
/// DTO для статистики дилеров по рынкам
/// </summary>
public record MarketDealerStatDto(
    string Market,
    int Count
);

/// <summary>
/// DTO для статистики дилеров
/// </summary>
public record DealerStatsSummaryDto(
    int TotalDealers,
    decimal AverageRating,
    List<DealerBriefDto> TopRated,
    List<MarketDealerStatDto> ByMarket
);
