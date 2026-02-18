using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        return services;
    }

    /// <summary>
    /// Добавить все парсеры
    /// </summary>
    public static IServiceCollection AddParsers(this IServiceCollection services)
    {
        services.AddChe168Parser();
        return services;
    }
}
