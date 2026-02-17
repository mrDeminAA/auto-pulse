namespace AutoPulse.Domain;

/// <summary>
/// Источник данных (сайт-донор)
/// </summary>
public class DataSource
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty; // Autohome, Cars.com
    public string Country { get; private set; } = string.Empty; // China, USA
    public string BaseUrl { get; private set; } = string.Empty;
    public string? Language { get; private set; } // zh-CN, en-US
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    private DataSource() { }

    public DataSource(string name, string country, string baseUrl, string? language = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название источника не может быть пустым", nameof(name));

        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("URL не может быть пустым", nameof(baseUrl));

        Name = name.Trim();
        Country = country.Trim();
        BaseUrl = baseUrl.Trim();
        Language = language;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
