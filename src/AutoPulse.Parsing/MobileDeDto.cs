namespace AutoPulse.Parsing;

/// <summary>
/// DTO для парсинга mobile.de (Европа)
/// </summary>
public class MobileDeCarDto
{
    public string? Title { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Price { get; set; }
    public string? Year { get; set; }
    public string? Mileage { get; set; }
    public string? Fuel { get; set; }
    public string? Transmission { get; set; }
    public string? Power { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ImageUrl { get; set; }
    public string? Url { get; set; }
    public DateTime ParsedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Результат поиска mobile.de
/// </summary>
public class MobileDeSearchResult
{
    public List<MobileDeCarDto> Cars { get; set; } = new();
    public int TotalResults { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage => CurrentPage < TotalPages;
}
