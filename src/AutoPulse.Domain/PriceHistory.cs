namespace AutoPulse.Domain;

/// <summary>
/// История изменения цены автомобиля
/// </summary>
public class PriceHistory
{
    public int Id { get; private set; }
    public int CarId { get; private set; }
    public decimal OldPrice { get; private set; }
    public string OldCurrency { get; private set; } = string.Empty;
    public decimal NewPrice { get; private set; }
    public string NewCurrency { get; private set; } = string.Empty;
    public DateTime RecordedAt { get; private set; }

    // Навигационные свойства
    public virtual Car Car { get; private set; } = null!;

    private PriceHistory() { }

    public PriceHistory(int carId, decimal oldPrice, string oldCurrency, decimal newPrice)
    {
        CarId = carId;
        OldPrice = oldPrice;
        OldCurrency = oldCurrency.ToUpper();
        NewPrice = newPrice;
        NewCurrency = oldCurrency.ToUpper();
        RecordedAt = DateTime.UtcNow;
    }
}
