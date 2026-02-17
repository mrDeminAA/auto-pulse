namespace AutoPulse.Domain;

/// <summary>
/// Дилер/продавец автомобилей
/// </summary>
public class Dealer
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? ContactInfo { get; private set; } // Email, телефон, сайт
    public string? Address { get; private set; }
    public decimal Rating { get; private set; } // 0-5
    public int MarketId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Навигационные свойства
    public virtual Market Market { get; private set; } = null!;
    public virtual ICollection<Car> Cars { get; private set; } = new List<Car>();

    private Dealer() { }

    public Dealer(string name, int marketId, string? contactInfo = null, string? address = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название дилера не может быть пустым", nameof(name));

        if (marketId <= 0)
            throw new ArgumentException("ID рынка должен быть положительным", nameof(marketId));

        Name = name.Trim();
        MarketId = marketId;
        ContactInfo = contactInfo;
        Address = address;
        Rating = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetRating(decimal rating)
    {
        if (rating < 0 || rating > 5)
            throw new ArgumentException("Рейтинг должен быть от 0 до 5", nameof(rating));

        Rating = rating;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContactInfo(string? contactInfo, string? address)
    {
        ContactInfo = contactInfo;
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }
}
