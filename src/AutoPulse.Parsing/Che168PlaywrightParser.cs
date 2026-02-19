using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Parsing;

/// <summary>
/// Парсер Che168 через Playwright (мобильная версия m.che168.com)
/// Использует headless браузер с iPhone User-Agent для доступа к мобильной версии
/// </summary>
public class Che168PlaywrightParser
{
    private readonly IBrowser _browser;
    private readonly ILogger<Che168PlaywrightParser> _logger;
    private readonly HashSet<string> _seenKeys = new();

    public Che168PlaywrightParser(IBrowser browser, ILogger<Che168PlaywrightParser> logger)
    {
        _browser = browser;
        _logger = logger;
    }

    /// <summary>
    /// Распарсить страницу поиска на мобильной версии che168.com
    /// </summary>
    public async Task<IReadOnlyList<ParsedCarData>> ParseSearchPageAsync(
        string brandUrl,
        int pageIndex = 1,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Playwright парсинг (mobile): {Url}, страница {Page}", brandUrl, pageIndex);

        try
        {
            var page = await _browser.NewPageAsync();

            try
            {
                // Формируем URL для мобильной версии
                var url = pageIndex == 1 
                    ? "https://m.che168.com/carlist/index?pvareaid=111478" 
                    : $"https://m.che168.com/carlist/index?pvareaid=111478&page={pageIndex}";

                // Переходим на страницу
                await page.GotoAsync(url, new() { WaitUntil = WaitUntilState.Load, Timeout = 120000 });

                // Ждём загрузки контента через скроллинг
                for (int i = 0; i < 8; i++)
                {
                    await page.WaitForTimeoutAsync(3000);
                    await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
                    await page.WaitForTimeoutAsync(1000);
                    await page.EvaluateAsync("window.scrollTo(0, 0)");

                    var text = await page.EvaluateAsync<string>("() => document.body.innerText");
                    _logger.LogDebug("Загрузка {Iteration}: {Length} символов", i + 1, text?.Length ?? 0);

                    if ((text?.Length ?? 0) > 1000) break;
                }

                // Получаем текст и изображения
                var pageText = await page.EvaluateAsync<string>("() => document.body.innerText");
                var cars = ParseCarsFromText(pageText ?? "");

                // Получаем изображения отдельно
                var images = await GetCarImagesAsync(page);

                // Сопоставляем изображения с автомобилями
                for (int i = 0; i < cars.Count && i < images.Count; i++)
                {
                    cars[i].ImageUrl = images[i];
                }

                _logger.LogInformation("Распаршено {Count} автомобилей", cars.Count);
                return cars;
            }
            finally
            {
                await page.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при парсинге {Url}", brandUrl);
            throw;
        }
    }

    /// <summary>
    /// Получить все автомобили с пагинацией
    /// </summary>
    public async IAsyncEnumerable<ParsedCarData> ParseAllAsync(
        string brandUrl,
        int maxPages = 20,
        int targetCount = 100,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        int page = 1;
        int totalParsed = 0;

        _logger.LogInformation("Начало парсинга: цель {Target} авто, макс. {MaxPages} страниц", targetCount, maxPages);

        while (page <= maxPages && totalParsed < targetCount && !cancellationToken.IsCancellationRequested)
        {
            var cars = await ParseSearchPageAsync(brandUrl, page, cancellationToken);

            // Фильтруем дубликаты
            var newCars = cars.Where(c => !_seenKeys.Contains(c.FullName + c.Price)).ToList();
            foreach (var car in newCars)
            {
                _seenKeys.Add(car.FullName + car.Price);
            }

            foreach (var car in newCars)
            {
                yield return car;
            }

            totalParsed += newCars.Count;
            page++;

            _logger.LogInformation("Страниц: {Page}, всего авто: {Total}", page - 1, totalParsed);

            if (newCars.Count == 0)
            {
                _logger.LogInformation("Больше нет автомобилей");
                yield break;
            }

            // Задержка между запросами
            await Task.Delay(2000, cancellationToken);
        }
    }

    private async Task<List<string>> GetCarImagesAsync(IPage page)
    {
        var images = new List<string>();

        try
        {
            var imageUrls = await page.EvaluateAsync<string[]>(@"() => {
                const imgs = [];
                document.querySelectorAll('img').forEach(img => {
                    const src = img.src || img.getAttribute('data-src') || img.getAttribute('data-original');
                    if (!src || !src.startsWith('http') || src.includes('data:')) return;
                    
                    // Фильтруем иконки
                    const skipPatterns = ['arrow_', 'icon', 'logo', 'global', 'tag_', 'filter', 'home_', 'appimg', 'chedan'];
                    if (skipPatterns.some(p => src.toLowerCase().includes(p))) return;
                    
                    imgs.push(src);
                });
                return imgs;
            }");

            if (imageUrls != null)
            {
                images = imageUrls.ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка получения изображений");
        }

        return images;
    }

    private List<ParsedCarData> ParseCarsFromText(string text)
    {
        var cars = new List<ParsedCarData>();
        var lines = text.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();

        var skipWords = new[] { 
            "精品车", "诚信车", "低价清仓", "春节不打烊", "官方补贴", "准新车", "原厂质保", "0 次过户", 
            "加载中...", "我知道了", "店铺", "分期", "排序", "品牌", "价格", "车龄", "筛选", 
            "搜索", "全部", "新能源", "会员商家", "全国", "新发捡漏", "0 出险", "90 天回购", 
            "检测报告", "有选装", "纯电动", "Global", "年终狂补", "4S 直卖", "日常代步", 
            "5 万以下", "新人练手", "15 万宝马", "成员商家", "首付", "补贴后", "万首付",
            "有礼赠送", "免手续费", "电池租赁", "续航", "km", "整备中", "支持本地购买"
        };

        ParsedCarData? currentCar = null;
        string? pendingPrice = null;

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            if (skipWords.Contains(line) || line.Length < 2)
                continue;

            // Число (первая часть цены)
            var numberMatch = Regex.Match(line, @"^([\d.]+)$");
            if (numberMatch.Success && i + 1 < lines.Count && lines[i + 1] == "万")
            {
                pendingPrice = numberMatch.Groups[1].Value + "万";
                continue;
            }

            // Цена
            string? priceValue = null;
            var priceMatch = Regex.Match(line, @"^([\d.]+)\s*万$");
            if (priceMatch.Success)
            {
                priceValue = priceMatch.Groups[1].Value + "万";
            }
            else if (pendingPrice != null)
            {
                priceValue = pendingPrice;
                pendingPrice = null;
            }

            if (priceValue != null)
            {
                if (currentCar != null && string.IsNullOrEmpty(currentCar.Price.ToString()))
                {
                    if (decimal.TryParse(priceValue.Replace("万", ""), out var price))
                    {
                        currentCar.Price = price;
                    }
                    cars.Add(currentCar);
                    currentCar = null;
                }
                continue;
            }

            // Год и пробег
            var ymMatch = Regex.Match(line, @"(\d{4}) 年\s*\/\s*([\d.]+)\s*万公里\s*\/\s*([^\s]+)");
            if (ymMatch.Success)
            {
                if (currentCar != null && string.IsNullOrEmpty(currentCar.Year.ToString()))
                {
                    currentCar.Year = int.Parse(ymMatch.Groups[1].Value);
                    currentCar.Mileage = (int)(double.Parse(ymMatch.Groups[2].Value) * 10000);
                    currentCar.City = ymMatch.Groups[3].Value;
                }
                else if (currentCar == null || !string.IsNullOrEmpty(currentCar.Price.ToString()))
                {
                    currentCar = new ParsedCarData
                    {
                        Year = int.Parse(ymMatch.Groups[1].Value),
                        Mileage = (int)(double.Parse(ymMatch.Groups[2].Value) * 10000),
                        City = ymMatch.Groups[3].Value
                    };
                }
                continue;
            }

            // Название
            if (line.Length > 5 && line.Length < 80 && currentCar == null)
            {
                currentCar = new ParsedCarData { FullName = line };
                
                // Парсим бренд и модель из названия
                var (brand, model, year) = ParseCarName(line);
                currentCar.Brand = brand;
                currentCar.Model = model;
                if (year > 0 && currentCar.Year == 0)
                {
                    currentCar.Year = year;
                }
            }
        }

        return cars.Where(c => !string.IsNullOrEmpty(c.FullName)).ToList();
    }

    private (string brand, string model, int year) ParseCarName(string? carName)
    {
        if (string.IsNullOrEmpty(carName))
            return ("", "", 0);

        var parts = carName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string brand = parts.Length > 0 ? parts[0] : "";
        string model = "";
        int year = 0;

        for (int i = 1; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.Contains("款") && int.TryParse(part.Replace("款", ""), out var y) && y >= 1900 && y <= DateTime.UtcNow.Year + 1)
            {
                year = y;
            }
            else if (part.Length > 1)
            {
                model += part + " ";
            }
        }

        return (brand, model.Trim(), year);
    }
}
