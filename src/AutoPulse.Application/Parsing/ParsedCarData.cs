namespace AutoPulse.Application.Parsing;

/// <summary>
/// Данные автомобиля с сайта-источника
/// </summary>
public class ParsedCarData
{
    public string BrandName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "CNY";
    public int Mileage { get; set; }
    public string? Vin { get; set; }
    public string? Transmission { get; set; }
    public string? Engine { get; set; }
    public string? FuelType { get; set; }
    public string? Color { get; set; }
    public string? Location { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? DealerName { get; set; }
    public string? DealerContact { get; set; }
}
