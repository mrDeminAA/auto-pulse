using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AutoPulse.Infrastructure.Services;

/// <summary>
/// Сервис конвертации валют
/// </summary>
public interface ICurrencyConversionService
{
    Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken = default);
    Task<decimal> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken = default);
    void InvalidateCache(string fromCurrency, string toCurrency);
}

/// <summary>
/// Реализация сервиса конвертации с кэшированием в Redis
/// </summary>
public class CurrencyConversionService : ICurrencyConversionService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CurrencyConversionService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    // Фиксированные курсы для основных валют (на случай недоступности API)
    private static readonly Dictionary<string, decimal> _fallbackRates = new()
    {
        { "CNY", 12.5m },  // Юань -> RUB (примерный курс)
        { "USD", 92.0m },  // Доллар -> RUB
        { "EUR", 99.0m },  // Евро -> RUB
        { "JPY", 0.61m },  // Иена -> RUB
        { "KRW", 0.065m }  // Вона -> RUB
    };

    public CurrencyConversionService(
        IDistributedCache cache,
        ILogger<CurrencyConversionService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _cache = cache;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken = default)
    {
        if (fromCurrency == toCurrency)
            return amount;

        var rate = await GetRateAsync(fromCurrency, toCurrency, cancellationToken);
        return amount * rate;
    }

    public async Task<decimal> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken = default)
    {
        // Нормализуем коды валют
        fromCurrency = fromCurrency.ToUpperInvariant();
        toCurrency = toCurrency.ToUpperInvariant();

        // Проверяем кэш
        var cacheKey = $"fx_rate:{fromCurrency}:{toCurrency}";
        var cachedRate = await _cache.GetStringAsync(cacheKey, cancellationToken);
        
        if (!string.IsNullOrEmpty(cachedRate) && decimal.TryParse(cachedRate, out var rate))
        {
            _logger.LogDebug("Курс {From}->{To} получен из кэша: {Rate}", fromCurrency, toCurrency, rate);
            return rate;
        }

        // Получаем курс через API ЦБ РФ
        rate = await FetchRateFromCentralBankAsync(fromCurrency, toCurrency, cancellationToken);
        
        if (rate > 0)
        {
            // Сохраняем в кэш
            await _cache.SetStringAsync(cacheKey, rate.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheDuration
            }, cancellationToken);
            
            _logger.LogInformation("Курс {From}->{To} получен из API: {Rate}", fromCurrency, toCurrency, rate);
            return rate;
        }

        // Используем fallback курс
        rate = GetFallbackRate(fromCurrency, toCurrency);
        _logger.LogWarning("Использую fallback курс {From}->{To}: {Rate}", fromCurrency, toCurrency, rate);
        
        return rate;
    }

    public void InvalidateCache(string fromCurrency, string toCurrency)
    {
        var cacheKey = $"fx_rate:{fromCurrency.ToUpperInvariant()}:{toCurrency.ToUpperInvariant()}";
        _cache.Remove(cacheKey);
        _logger.LogDebug("Кэш курса валют очищен: {Key}", cacheKey);
    }

    private async Task<decimal> FetchRateFromCentralBankAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("CentralBank");
            
            // Получаем курс к RUB через API ЦБ РФ
            var url = $"https://www.cbr-xml-ds.ru/api/ExchangeRateOnDate?date={DateTime.UtcNow:yyyy-MM-dd}&ValCode={fromCurrency}";
            
            var response = await client.GetAsync(url, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                // Парсим XML ответ ЦБ
                var rate = ParseCbrResponse(content, fromCurrency);
                if (rate > 0)
                {
                    // Если целевая валюта не RUB, конвертируем через кросс-курс
                    if (toCurrency != "RUB")
                    {
                        var targetRate = await GetRateAsync(toCurrency, "RUB", cancellationToken);
                        if (targetRate > 0)
                        {
                            rate = rate / targetRate;
                        }
                    }
                    return rate;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка получения курса из ЦБ РФ {From}->{To}", fromCurrency, toCurrency);
        }

        // Пробует альтернативный API
        return await FetchRateFromAlternativeApiAsync(fromCurrency, toCurrency, cancellationToken);
    }

    private async Task<decimal> FetchRateFromAlternativeApiAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        try
        {
            // Используем бесплатный API для курсов валют
            var client = _httpClientFactory.CreateClient("ExchangeRate");
            var url = $"https://api.exchangerate-api.com/v4/latest/{fromCurrency}";
            
            var response = await client.GetAsync(url, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("rates", out var rates) && 
                    rates.TryGetProperty(toCurrency, out var rateElement))
                {
                    return rateElement.GetDecimal();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка получения курса из альтернативного API {From}->{To}", fromCurrency, toCurrency);
        }

        return 0;
    }

    private decimal ParseCbrResponse(string xml, string currency)
    {
        try
        {
            // Простой парсинг XML ЦБ РФ
            var rateTag = $"<{currency}>";
            var startIdx = xml.IndexOf(rateTag);
            if (startIdx >= 0)
            {
                var endIdx = xml.IndexOf($"</{currency}>", startIdx);
                if (endIdx > startIdx)
                {
                    var rateStr = xml.Substring(startIdx + rateTag.Length, endIdx - startIdx - rateTag.Length);
                    // ЦБ использует запятую как десятичный разделитель
                    rateStr = rateStr.Replace(',', '.');
                    if (decimal.TryParse(rateStr, out var rate))
                    {
                        return rate;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка парсинга ответа ЦБ РФ");
        }
        return 0;
    }

    private decimal GetFallbackRate(string fromCurrency, string toCurrency)
    {
        // Конвертируем через RUB как базовую валюту
        if (_fallbackRates.TryGetValue(fromCurrency, out var fromRate) &&
            _fallbackRates.TryGetValue(toCurrency, out var toRate))
        {
            return fromRate / toRate;
        }

        // Возвращаем 1 если не нашли курс
        return 1m;
    }
}

/// <summary>
/// Расширение для регистрации сервиса конвертации валют
/// </summary>
public static class CurrencyConversionServiceCollectionExtensions
{
    public static IServiceCollection AddCurrencyConversion(this IServiceCollection services)
    {
        services.AddHttpClient("CentralBank", client =>
        {
            client.BaseAddress = new Uri("https://www.cbr-xml-ds.ru/");
            client.DefaultRequestHeaders.Add("Accept", "application/xml");
        });

        services.AddHttpClient("ExchangeRate", client =>
        {
            client.BaseAddress = new Uri("https://api.exchangerate-api.com/");
        });

        services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();
        return services;
    }
}
