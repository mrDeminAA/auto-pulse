namespace AutoPulse.Domain;

/// <summary>
/// Избранное пользователя (сохранённые автомобили)
/// </summary>
public class UserCar
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int CarId { get; private set; }
    public string? Note { get; private set; } // Заметка пользователя
    public bool IsFavorite { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Навигационные свойства
    public virtual User User { get; private set; } = null!;
    public virtual Car Car { get; private set; } = null!;

    private UserCar() { }

    public UserCar(int userId, int carId)
    {
        UserId = userId;
        CarId = carId;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetNote(string? note)
    {
        Note = note;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetFavorite(bool isFavorite)
    {
        IsFavorite = isFavorite;
        UpdatedAt = DateTime.UtcNow;
    }
}
