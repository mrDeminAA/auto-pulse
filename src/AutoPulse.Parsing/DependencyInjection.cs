using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace AutoPulse.Parsing;

/// <summary>
/// Расширения для регистрации парсеров в DI
/// </summary>
public static class ParsingServiceCollectionExtensions
{
    /// <summary>
    /// Добавить парсер Che168
    /// </summary>
    public static IServiceCollection AddChe168Parser(this IServiceCollection services)
    {
        // API Parser (не работает без _sign)
        services.AddHttpClient<Che168ApiParser>((provider, client) =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        // HTML Parser (рабочий!)
        services.AddHttpClient<Che168HtmlParser>((provider, client) =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
            );
        });

        // Playwright Parser (мобильная версия)
        services.AddSingleton<IPlaywright>(sp => Playwright.CreateAsync().GetAwaiter().GetResult());
        services.AddSingleton<IBrowser>(sp =>
        {
            var playwright = sp.GetRequiredService<IPlaywright>();
            return playwright.Chromium.LaunchAsync(new()
            {
                Headless = false,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            }).GetAwaiter().GetResult();
        });
        services.AddSingleton(sp =>
        {
            var browser = sp.GetRequiredService<IBrowser>();
            var logger = sp.GetRequiredService<ILogger<Che168PlaywrightParser>>();
            return new Che168PlaywrightParser(browser, logger);
        });

        return services;
    }

    /// <summary>
    /// Добавить парсер Mobile.de (Европа)
    /// </summary>
    public static IServiceCollection AddMobileDeParser(this IServiceCollection services)
    {
        services.AddSingleton<MobileDeParser>(sp =>
        {
            var browser = sp.GetRequiredService<IBrowser>();
            var logger = sp.GetRequiredService<ILogger<MobileDeParser>>();
            return new MobileDeParser(browser, logger);
        });

        return services;
    }

    /// <summary>
    /// Добавить парсер Cars.com (США)
    /// </summary>
    public static IServiceCollection AddCarsComParser(this IServiceCollection services)
    {
        services.AddSingleton<CarsComParser>(sp =>
        {
            var browser = sp.GetRequiredService<IBrowser>();
            var logger = sp.GetRequiredService<ILogger<CarsComParser>>();
            return new CarsComParser(browser, logger);
        });

        return services;
    }

    /// <summary>
    /// Добавить все парсеры
    /// </summary>
    public static IServiceCollection AddParsers(this IServiceCollection services)
    {
        services.AddChe168Parser();
        services.AddMobileDeParser();
        services.AddCarsComParser();
        return services;
    }
}
