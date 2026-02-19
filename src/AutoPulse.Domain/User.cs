namespace AutoPulse.Domain;

/// <summary>
/// Пользователь системы
/// </summary>
public class User
{
    public int Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? Name { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? TelegramId { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    // Навигационные свойства
    public virtual UserPreferences? Preferences { get; private set; }
    public virtual ICollection<UserCar> FavoriteCars { get; private set; } = new List<UserCar>();
    public virtual ICollection<CarAlert> CarAlerts { get; private set; } = new List<CarAlert>();

    private User() { }

    public User(string email, string passwordHash, string? name = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email не может быть пустым", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Пароль не может быть пустым", nameof(passwordHash));

        Email = email.Trim().ToLower();
        PasswordHash = passwordHash;
        Name = name?.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string? name = null, string? avatarUrl = null)
    {
        if (name != null)
            Name = name.Trim();
        
        if (avatarUrl != null)
            AvatarUrl = avatarUrl;
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTelegramId(string? telegramId)
    {
        TelegramId = telegramId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsLoggedIn()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Enable()
    {
        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
