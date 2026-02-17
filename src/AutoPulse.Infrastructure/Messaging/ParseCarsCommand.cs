using AutoPulse.Application.Parsing;

namespace AutoPulse.Infrastructure.Messaging;

/// <summary>
/// Сообщение для парсинга автомобилей
/// </summary>
public record ParseCarsCommand(
    string Url,
    string SourceName,
    string Country,
    int? MarketId = null
);

/// <summary>
/// Результат парсинга
/// </summary>
public record CarsParsedEvent(
    string SourceName,
    int ParsedCount,
    DateTime ParsedAt,
    List<AutoPulse.Application.Parsing.ParsedCarData> Cars
);
