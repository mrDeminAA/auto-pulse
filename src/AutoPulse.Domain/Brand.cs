namespace AutoPulse.Domain;

/// <summary>
/// Марка автомобиля (Toyota, BMW, Mercedes и т.д.)
/// </summary>
public class Brand
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Country { get; private set; }
    public string? LogoUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Навигационные свойства
    public virtual ICollection<Model> Models { get; private set; } = new List<Model>();

    private Brand() { }

    public Brand(string name, string? country = null)
    {
        SetName(name);
        Country = country;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название марки не может быть пустым", nameof(name));

        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetLogoUrl(string? logoUrl)
    {
        LogoUrl = logoUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}
