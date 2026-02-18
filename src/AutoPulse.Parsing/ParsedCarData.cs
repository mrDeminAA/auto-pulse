namespace AutoPulse.Parsing;

/// <summary>
/// Данные автомобиля с парсера
/// </summary>
public class ParsedCarData
{
    /// <summary>
    /// Уникальный идентификатор объявления
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Бренд (например, "奥迪")
    /// </summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>
    /// Модель (например, "A4L")
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Полное название (например, "奥迪 A4L 2016 款 45 TFSI")
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Год выпуска
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Цена в юанях
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Пробег в км
    /// </summary>
    public int Mileage { get; set; }

    /// <summary>
    /// Город
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Дилер
    /// </summary>
    public string Dealer { get; set; } = string.Empty;

    /// <summary>
    /// Двигатель
    /// </summary>
    public string? Displacement { get; set; }

    /// <summary>
    /// URL изображения
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// URL источника
    /// </summary>
    public string SourceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Дата парсинга
    /// </summary>
    public DateTime ParsedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Источник (например, "Che168")
    /// </summary>
    public string Source { get; set; } = string.Empty;
}
