namespace AutoPulse.Application.Parsing;

/// <summary>
/// Интерфейс парсера автомобилей
/// </summary>
public interface ICarParser
{
    /// <summary>
    /// Название источника (например, "Autohome")
    /// </summary>
    string SourceName { get; }

    /// <summary>
    /// Страна источника (например, "China")
    /// </summary>
    string Country { get; }

    /// <summary>
    /// Парсинг списка автомобилей со страницы
    /// </summary>
    Task<IReadOnlyList<ParsedCarData>> ParseListAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Парсинг деталей автомобиля
    /// </summary>
    Task<ParsedCarData?> ParseDetailsAsync(string url, CancellationToken cancellationToken = default);
}
