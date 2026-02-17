namespace AutoPulse.Domain;

/// <summary>
/// Рынок (регион): USA, Europe, China, Korea, Japan
/// </summary>
public class Market
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty; // USA, Europe, China
    public string Region { get; private set; } = string.Empty; // North America, Europe, Asia
    public string Currency { get; private set; } = string.Empty; // USD, EUR, CNY

    public DateTime CreatedAt { get; private set; }

    // Навигационные свойства
    public virtual ICollection<Car> Cars { get; private set; } = new List<Car>();

    private Market() { }

    public Market(string name, string region, string currency)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название рынка не может быть пустым", nameof(name));

        if (string.IsNullOrWhiteSpace(region))
            throw new ArgumentException("Регион не может быть пустым", nameof(region));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Валюта не может быть пустой", nameof(currency));

        Name = name.Trim();
        Region = region.Trim();
        Currency = currency.Trim().ToUpper();
        CreatedAt = DateTime.UtcNow;
    }
}
