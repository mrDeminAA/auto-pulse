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
        services.AddHttpClient<Che168ApiParser>((provider, client) =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
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
