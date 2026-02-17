namespace AutoPulse.Domain;

/// <summary>
/// Автомобиль
/// </summary>
public class Car
{
    public int Id { get; private set; }
    public int BrandId { get; private set; }
    public int ModelId { get; private set; }
    public int MarketId { get; private set; }
    public int? DealerId { get; private set; }

    // Основная информация
    public int Year { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string? Vin { get; private set; }
    public int Mileage { get; private set; } // в км
    public string? Transmission { get; private set; } // Automatic, Manual
    public string? Engine { get; private set; } // 2.5L, V6 и т.д.
    public string? FuelType { get; private set; } // Gasoline, Diesel, Electric, Hybrid
    public string? Color { get; private set; }

    // Расположение
    public string? Location { get; private set; } // Город, штат
    public string? Country { get; private set; }

    // URL и изображения
    public string SourceUrl { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }

    // Статус
    public bool IsAvailable { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? SoldAt { get; private set; }

    // Навигационные свойства
    public virtual Brand Brand { get; private set; } = null!;
    public virtual Model Model { get; private set; } = null!;
    public virtual Market Market { get; private set; } = null!;
    public virtual Dealer? Dealer { get; private set; }

    private Car() { }

    public Car(
        int brandId,
        int modelId,
        int marketId,
        int year,
        decimal price,
        string currency,
        string sourceUrl)
    {
        if (brandId <= 0)
            throw new ArgumentException("ID марки должен быть положительным", nameof(brandId));

        if (modelId <= 0)
            throw new ArgumentException("ID модели должен быть положительным", nameof(modelId));

        if (marketId <= 0)
            throw new ArgumentException("ID рынка должен быть положительным", nameof(marketId));

        if (year < 1900 || year > DateTime.UtcNow.Year + 1)
            throw new ArgumentException("Некорректный год", nameof(year));

        if (price < 0)
            throw new ArgumentException("Цена должна быть положительной", nameof(price));

        if (string.IsNullOrWhiteSpace(sourceUrl))
            throw new ArgumentException("URL источника обязателен", nameof(sourceUrl));

        BrandId = brandId;
        ModelId = modelId;
        MarketId = marketId;
        Year = year;
        Price = price;
        Currency = currency.ToUpper();
        SourceUrl = sourceUrl;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetVin(string? vin)
    {
        Vin = vin?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDetails(
        int mileage,
        string? transmission = null,
        string? engine = null,
        string? fuelType = null,
        string? color = null)
    {
        if (mileage < 0)
            throw new ArgumentException("Пробег не может быть отрицательным", nameof(mileage));

        Mileage = mileage;
        Transmission = transmission;
        Engine = engine;
        FuelType = fuelType;
        Color = color;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetLocation(string? location, string? country)
    {
        Location = location;
        Country = country;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetImageUrl(string? imageUrl)
    {
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDealer(int? dealerId)
    {
        if (dealerId.HasValue && dealerId <= 0)
            throw new ArgumentException("ID дилера должен быть положительным", nameof(dealerId));

        DealerId = dealerId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsSold()
    {
        IsAvailable = false;
        SoldAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Цена должна быть положительной", nameof(price));

        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }
}
