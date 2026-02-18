namespace AutoPulse.Parsing;

/// <summary>
/// DTO для ответа API Che168
/// </summary>
internal class Che168ApiResponse
{
    public int ReturnCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public Che168ApiResult? Result { get; set; }
}

internal class Che168ApiResult
{
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int PageIndex { get; set; }
    public int PageCount { get; set; }
    public List<Che168CarDto>? CarList { get; set; }
}

internal class Che168CarDto
{
    public long Infoid { get; set; }
    public string? Carname { get; set; }
    public string? Price { get; set; }
    public string? Mileage { get; set; }
    public string? Cname { get; set; }
    public string? Firstregyear { get; set; }
    public string? DealerLevel { get; set; }
    public string? Imageurl { get; set; }
    public string? ImageUrl_800 { get; set; }  // Большое изображение
    public string? Displacement { get; set; }
}
