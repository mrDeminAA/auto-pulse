namespace AutoPulse.Domain;

/// <summary>
/// Предпочтения пользователя для поиска автомобиля
/// </summary>
public class UserPreferences
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    
    // Параметры автомобиля
    public int? BrandId { get; private set; }
    public int? ModelId { get; private set; }
    public string? Generation { get; private set; } // Например: "8P", "8V", "8Y" для Audi A3
    public string? BodyTypes { get; private set; } // JSON: ["hatchback", "sedan"]
    public string? Engines { get; private set; } // JSON: ["1.4_tfsi", "2.0_tfsi"]
    public string? Transmission { get; private set; } // "manual", "s-tronic", "any"
    
    // Год и пробег
    public int? YearFrom { get; private set; }
    public int? YearTo { get; private set; }
    public int? MaxMileage { get; private set; }
    
    // Бюджет (в рублях)
    public decimal? BudgetFrom { get; private set; }
    public decimal? BudgetTo { get; private set; }
    
    // Регионы поиска
    public string? Regions { get; private set; } // JSON: ["china", "europe", "usa"]
    
    // Настройки уведомлений
    public bool NotifyOnNew { get; private set; } = true;
    public bool NotifyOnPriceDrop { get; private set; } = true;
    public decimal? PriceDropThreshold { get; private set; } // Мин. снижение цены для уведомления (в %)
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Навигационные свойства
    public virtual User User { get; private set; } = null!;
    public virtual Brand? Brand { get; private set; }
    public virtual Model? Model { get; private set; }

    private UserPreferences() { }

    public UserPreferences(int userId)
    {
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetCarPreferences(
        int? brandId = null,
        int? modelId = null,
        string? generation = null,
        string? bodyTypes = null,
        string? engines = null,
        string? transmission = null)
    {
        BrandId = brandId;
        ModelId = modelId;
        Generation = generation;
        BodyTypes = bodyTypes;
        Engines = engines;
        Transmission = transmission;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetYearRange(int? from, int? to)
    {
        if (from.HasValue && to.HasValue && from > to)
            throw new ArgumentException("Год 'от' не может быть больше года 'до'", nameof(from));

        YearFrom = from;
        YearTo = to;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetBudget(decimal? from, decimal? to)
    {
        if (from.HasValue && to.HasValue && from > to)
            throw new ArgumentException("Бюджет 'от' не может быть больше бюджета 'до'", nameof(from));

        BudgetFrom = from;
        BudgetTo = to;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMaxMileage(int? mileage)
    {
        if (mileage.HasValue && mileage < 0)
            throw new ArgumentException("Пробег не может быть отрицательным", nameof(mileage));

        MaxMileage = mileage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRegions(string? regionsJson)
    {
        Regions = regionsJson;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetNotificationSettings(
        bool notifyOnNew = true,
        bool notifyOnPriceDrop = true,
        decimal? priceDropThreshold = null)
    {
        NotifyOnNew = notifyOnNew;
        NotifyOnPriceDrop = notifyOnPriceDrop;
        PriceDropThreshold = priceDropThreshold;
        UpdatedAt = DateTime.UtcNow;
    }
}
