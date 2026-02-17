namespace AutoPulse.Domain;

/// <summary>
/// Модель автомобиля (Camry, X5, Golf и т.д.)
/// </summary>
public class Model
{
    public int Id { get; private set; }
    public int BrandId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Category { get; private set; } // Sedan, SUV, Hatchback и т.д.
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Навигационные свойства
    public virtual Brand Brand { get; private set; } = null!;
    public virtual ICollection<Car> Cars { get; private set; } = new List<Car>();

    private Model() { }

    public Model(int brandId, string name, string? category = null)
    {
        if (brandId <= 0)
            throw new ArgumentException("ID марки должен быть положительным", nameof(brandId));

        SetName(name);
        BrandId = brandId;
        Category = category;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название модели не может быть пустым", nameof(name));

        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCategory(string? category)
    {
        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }
}
