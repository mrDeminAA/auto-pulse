namespace AutoPulse.Domain;

/// <summary>
/// Поисковый запрос пользователя (какую машину ищет)
/// </summary>
public class UserSearch
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int? BrandId { get; internal set; }
    public int? ModelId { get; internal set; }
    public string? Generation { get; private set; }
    public int? YearFrom { get; private set; }
    public int? YearTo { get; private set; }
    public decimal? MaxPrice { get; private set; }
    public int? MaxMileage { get; private set; }
    public string? Regions { get; private set; }
    public SearchStatus Status { get; private set; } = SearchStatus.Active;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastNotifiedAt { get; private set; }

    // Навигационные свойства
    public virtual User User { get; private set; } = null!;
    public virtual Brand? Brand { get; private set; }
    public virtual Model? Model { get; private set; }
    public virtual ICollection<UserSearchQueue> SearchQueues { get; private set; } = new List<UserSearchQueue>();

    private UserSearch() { }

    public UserSearch(int userId, int? brandId = null, int? modelId = null)
    {
        UserId = userId;
        BrandId = brandId;
        ModelId = modelId;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetParameters(
        int? brandId = null,
        int? modelId = null,
        string? generation = null,
        int? yearFrom = null,
        int? yearTo = null,
        decimal? maxPrice = null,
        int? maxMileage = null,
        string? regions = null)
    {
        if (brandId.HasValue)
            BrandId = brandId;
        if (modelId.HasValue)
            ModelId = modelId;
        Generation = generation;
        YearFrom = yearFrom;
        YearTo = yearTo;
        MaxPrice = maxPrice;
        MaxMileage = maxMileage;
        Regions = regions;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStatus(SearchStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsNotified()
    {
        LastNotifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum SearchStatus
{
    Active = 1,       // Активный поиск
    Paused = 2,       // На паузе
    Completed = 3,    // Завершён (нашли что искали)
    Cancelled = 4     // Отменён пользователем
}
