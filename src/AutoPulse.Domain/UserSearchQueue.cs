namespace AutoPulse.Domain;

/// <summary>
/// Связь между поиском пользователя и очередью парсинга
/// </summary>
public class UserSearchQueue
{
    public int Id { get; private set; }
    public int UserSearchId { get; private set; }
    public int CarSearchQueueId { get; private set; }
    public bool IsNewCarsNotified { get; private set; } = false;
    public bool IsPriceDropNotified { get; private set; } = false;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Навигационные свойства
    public virtual UserSearch UserSearch { get; private set; } = null!;
    public virtual CarSearchQueue CarSearchQueue { get; private set; } = null!;

    private UserSearchQueue() { }

    public UserSearchQueue(int userSearchId, int carSearchQueueId)
    {
        UserSearchId = userSearchId;
        CarSearchQueueId = carSearchQueueId;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkNewCarsNotified()
    {
        IsNewCarsNotified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkPriceDropNotified()
    {
        IsPriceDropNotified = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
