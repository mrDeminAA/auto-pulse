namespace AutoPulse.Domain;

/// <summary>
/// Очередь парсинга (уникальные комбинации бренд/модель/год)
/// Один запрос может обслуживать нескольких пользователей
/// </summary>
public class CarSearchQueue
{
    public int Id { get; private set; }
    public int? BrandId { get; private set; }
    public int? ModelId { get; private set; }
    public string? Generation { get; private set; }
    public int YearFrom { get; private set; }
    public int YearTo { get; private set; }
    public string Regions { get; private set; } = string.Empty; // JSON: ["china", "europe"]
    public QueueStatus Status { get; private set; } = QueueStatus.Pending;
    public int Priority { get; private set; } // Количество пользователей ждущих
    public int ConsecutiveFailures { get; private set; } = 0;
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastParsedAt { get; private set; }
    public DateTime? NextParseAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? LastError { get; private set; }

    // Навигационные свойства
    public virtual Brand? Brand { get; private set; }
    public virtual Model? Model { get; private set; }
    public virtual ICollection<UserSearchQueue> UserSearches { get; private set; } = new List<UserSearchQueue>();

    private CarSearchQueue() { }

    public CarSearchQueue(
        int yearFrom,
        int yearTo,
        string regions,
        int? brandId = null,
        int? modelId = null,
        string? generation = null)
    {
        BrandId = brandId;
        ModelId = modelId;
        Generation = generation;
        YearFrom = yearFrom;
        YearTo = yearTo;
        Regions = regions;
        Priority = 0;
        CreatedAt = DateTime.UtcNow;
        NextParseAt = DateTime.UtcNow;
    }

    public void StartParsing()
    {
        Status = QueueStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CompleteParsing(int carsFound)
    {
        Status = QueueStatus.Completed;
        LastParsedAt = DateTime.UtcNow;
        ConsecutiveFailures = 0;
        UpdatedAt = DateTime.UtcNow;

        // Расчёт следующего парсинга на основе приоритета
        int minutesUntilNext = Priority switch
        {
            >= 10 => 15,    // Очень высокий спрос - каждые 15 мин
            >= 5 => 30,     // Высокий спрос - каждые 30 мин
            >= 2 => 60,     // Средний спрос - каждые 1 час
            _ => 120        // Низкий спрос - каждые 2 часа
        };

        NextParseAt = DateTime.UtcNow.AddMinutes(minutesUntilNext);
    }

    public void FailParsing(string errorMessage)
    {
        Status = QueueStatus.Failed;
        LastError = errorMessage;
        ConsecutiveFailures++;
        UpdatedAt = DateTime.UtcNow;

        // Следующая попытка через экспоненциальную задержку
        int minutesUntilRetry = Math.Min(5 * (int)Math.Pow(2, ConsecutiveFailures), 120);
        NextParseAt = DateTime.UtcNow.AddMinutes(minutesUntilRetry);
    }

    public void IncrementPriority()
    {
        Priority++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecrementPriority()
    {
        if (Priority > 0)
            Priority--;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetNextParse(TimeSpan delay)
    {
        NextParseAt = DateTime.UtcNow + delay;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum QueueStatus
{
    Pending = 1,      // В очереди
    Processing = 2,   // Сейчас парсится
    Completed = 3,    // Успешно распаршено
    Failed = 4        // Ошибка
}
