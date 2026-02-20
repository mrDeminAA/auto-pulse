using Microsoft.Playwright;
using AutoPulse.Parsing;
using Microsoft.Extensions.Logging;

Console.WriteLine("=== AutoPulse Parser Test ===\n");
Console.WriteLine("Запуск теста Che168 (Китай)...\n");

try
{
    // Создаем Playwright
    var playwright = await Playwright.CreateAsync();

    // Запускаем браузер
    var browser = await playwright.Chromium.LaunchAsync(new()
    {
        Headless = false,
        Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
    });

    // Создаем логгер
    var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

    await TestChe168Async(browser, loggerFactory);

    await browser.CloseAsync();
    playwright.Dispose();

    Console.WriteLine("\n✅ Тест завершен!");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Ошибка: {ex.Message}");
    Console.WriteLine($"\n{ex.StackTrace}");
}

Console.WriteLine("\nНажмите Enter для выхода...");
Console.ReadLine();

async Task TestChe168Async(IBrowser browser, ILoggerFactory loggerFactory)
{
    Console.WriteLine("🇨🇳 === Тест Che168 (Китай) ===\n");

    var logger = loggerFactory.CreateLogger<Che168PlaywrightParser>();
    var parser = new Che168PlaywrightParser(browser, logger);

    var url = "https://m.che168.com/carlist/index?pvareaid=111478";
    Console.WriteLine($"Парсинг URL: {url}");

    var result = await parser.ParseSearchPageAsync(url, 1);

    Console.WriteLine($"\n✅ Найдено автомобилей: {result.Count}\n");

    foreach (var (car, i) in result.Take(5).Select((c, idx) => (c, idx + 1)))
    {
        Console.WriteLine($"{i}. {car.FullName}");
        Console.WriteLine($"   Цена: {car.Price} CNY (~{car.Price * 13.5m:N0} RUB)");
        Console.WriteLine($"   Год: {car.Year}, Пробег: {car.Mileage:N0} км, Город: {car.City}");
        Console.WriteLine();
    }
}

async Task TestMobileDeAsync(IBrowser browser, ILoggerFactory loggerFactory)
{
    Console.WriteLine("🇪🇺 === Тест Mobile.de (Европа) ===\n");

    var logger = loggerFactory.CreateLogger<MobileDeParser>();
    var parser = new MobileDeParser(browser, logger);

    var url = "https://www.mobile.de/autos?make=audi&model=a3&damagedLst=false&isSearchRequest=true&sfct=false";
    Console.WriteLine($"Парсинг URL: {url}");

    var result = await parser.ParseSearchPageAsync(url, 1);

    Console.WriteLine($"\n✅ Найдено автомобилей: {result.Cars.Count}\n");

    foreach (var (car, i) in result.Cars.Take(5).Select((c, idx) => (c, idx + 1)))
    {
        Console.WriteLine($"{i}. {car.Title}");
        Console.WriteLine($"   Цена: {car.Price} EUR (~{decimal.Parse(car.Price ?? "0") * 100m:N0} RUB)");
        Console.WriteLine($"   Год: {car.Year}, Пробег: {car.Mileage} км");
        Console.WriteLine($"   Город: {car.City}, {car.Country}");
        Console.WriteLine($"   Топливо: {car.Fuel}, Коробка: {car.Transmission}");
        Console.WriteLine();
    }
}

async Task TestCarsComAsync(IBrowser browser, ILoggerFactory loggerFactory)
{
    Console.WriteLine("🇺🇸 === Тест Cars.com (США) ===\n");

    var logger = loggerFactory.CreateLogger<CarsComParser>();
    var parser = new CarsComParser(browser, logger);

    var url = "https://www.cars.com/shopping/audi/a3/?page_size=20&zip=10001&distance=99999";
    Console.WriteLine($"Парсинг URL: {url}");

    var result = await parser.ParseSearchPageAsync(url, 1);

    Console.WriteLine($"\n✅ Найдено автомобилей: {result.Cars.Count}\n");

    foreach (var (car, i) in result.Cars.Take(5).Select((c, idx) => (c, idx + 1)))
    {
        Console.WriteLine($"{i}. {car.Title}");
        Console.WriteLine($"   Цена: ${car.Price} USD (~{decimal.Parse(car.Price ?? "0") * 90m:N0} RUB)");
        Console.WriteLine($"   Год: {car.Year}, Пробег: {car.Mileage} mi");
        Console.WriteLine($"   Город: {car.City}, {car.State}");
        Console.WriteLine($"   Топливо: {car.Fuel}, Коробка: {car.Transmission}");
        Console.WriteLine();
    }
}
