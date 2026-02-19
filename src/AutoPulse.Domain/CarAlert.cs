namespace AutoPulse.Domain;

/// <summary>
/// Уведомление пользователя о новом автомобиле или изменении цены
/// </summary>
public class CarAlert
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int? CarId { get; private set; } // null если уведомление о общем совпадении
    public AlertType Type { get; private set; }
    public bool IsRead { get; private set; } = false;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    // Навигационные свойства
    public virtual User User { get; private set; } = null!;
    public virtual Car? Car { get; private set; }

    private CarAlert() { }

    public CarAlert(int userId, AlertType type, int? carId = null)
    {
        UserId = userId;
        Type = type;
        CarId = carId;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum AlertType
{
    NewCar = 1,        // Новый автомобиль по параметрам
    PriceDrop = 2,     // Снижение цены
    BackInStock = 3    // Снова в наличии
}
