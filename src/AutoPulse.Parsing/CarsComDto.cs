namespace AutoPulse.Parsing;

/// <summary>
/// DTO для парсинга cars.com (США)
/// </summary>
public class CarsComCarDto
{
    public string? Title { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Year { get; set; }
    public string? Price { get; set; }
    public string? Mileage { get; set; }
    public string? Fuel { get; set; }
    public string? Transmission { get; set; }
    public string? Drivetrain { get; set; }
    public string? Engine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? ImageUrl { get; set; }
    public string? Url { get; set; }
    public string? DealerName { get; set; }
    public DateTime ParsedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Результат поиска cars.com
/// </summary>
public class CarsComSearchResult
{
    public List<CarsComCarDto> Cars { get; set; } = new();
    public int TotalResults { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage => CurrentPage < TotalPages;
}
