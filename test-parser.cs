using Microsoft.Playwright;
using AutoPulse.Parsing;
using Microsoft.Extensions.Logging;

// Простой тест парсера
Console.WriteLine("=== Тест Che168 Playwright Parser ===\n");

// Создаем Playwright
var playwright = await Playwright.CreateAsync();

// Запускаем браузер
var browser = await playwright.Chromium.LaunchAsync(new()
{
    Headless = false,
    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
});

// Создаем контекст с мобильным User-Agent
var context = await browser.NewContextAsync(new()
{
    ViewportSize = new ViewportSize { Width = 414, Height = 896 },
    UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1",
    Locale = "zh-CN"
});

// Создаем логгер
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
var logger = loggerFactory.CreateLogger<Che168PlaywrightParser>();

// Создаем парсер
var parser = new Che168PlaywrightParser(browser, logger);

try
{
    Console.WriteLine("Парсинг первой страницы...");
    var cars = await parser.ParseSearchPageAsync("https://m.che168.com/carlist/index?pvareaid=111478", 1);
    
    Console.WriteLine($"\nНайдено автомобилей: {cars.Count}\n");
    
    foreach (var (car, i) in cars.Take(10).Select((c, idx) => (c, idx + 1)))
    {
        Console.WriteLine($"{i}. {car.FullName}");
        Console.WriteLine($"   Цена: {car.Price}, Год: {car.Year}, Пробег: {car.Mileage}, Город: {car.City}");
        if (!string.IsNullOrEmpty(car.ImageUrl))
        {
            var imgDisplay = car.ImageUrl.Length > 70 ? car.ImageUrl.Substring(0, 70) + "..." : car.ImageUrl;
            Console.WriteLine($"   Картинка: {imgDisplay}");
        }
        Console.WriteLine();
    }
    
    Console.WriteLine("\nНажмите Enter для выхода...");
    Console.ReadLine();
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
finally
{
    await browser.CloseAsync();
    playwright.Dispose();
}
