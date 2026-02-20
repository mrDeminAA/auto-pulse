# Парсеры AutoPulse

## Обзор

AutoPulse использует Playwright для парсинга автомобильных сайтов по всему миру.

---

## Источники

| Регион | Источник | Статус | Валюта | URL |
|--------|----------|--------|--------|-----|
| Китай | Che168 (мобильная) | ✅ Готово | CNY | m.che168.com |
| Европа | Mobile.de | ✅ Готово | EUR | www.mobile.de |
| США | Cars.com | ✅ Готово | USD | www.cars.com |

---

## Che168PlaywrightParser (Китай)

### Особенности:
- Мобильная версия m.che168.com
- iPhone User-Agent
- Умный скроллинг для загрузки контента
- Парсинг из текста (без сложных селекторов)

### Пример использования:

```csharp
var parser = new Che168PlaywrightParser(browser, logger);

// Парсинг одной страницы
var cars = await parser.ParseSearchPageAsync(
    "https://m.che168.com/carlist/index?pvareaid=111478", 
    pageIndex: 1
);

// Парсинг всех страниц
await foreach (var car in parser.ParseAllAsync(url, maxPages: 10, targetCount: 100))
{
    Console.WriteLine($"{car.FullName} - {car.Price} CNY");
}
```

---

## MobileDeParser (Европа)

### Особенности:
- Десктопная версия www.mobile.de
- Селекторы: `[data-testid='vehicle-card']`
- Парсинг деталей: цена, год, пробег, топливо, коробка
- Поддержка пагинации

### Пример использования:

```csharp
var parser = new MobileDeParser(browser, logger);

// Поиск Audi A3
var searchUrl = "https://www.mobile.de/autos?make=audi&model=a3";

var result = await parser.ParseSearchPageAsync(searchUrl, pageIndex: 1);

foreach (var car in result.Cars)
{
    Console.WriteLine($"{car.Title} - {car.Price} EUR");
    Console.WriteLine($"  {car.Year}, {car.Mileage} km, {car.Fuel}");
}
```

### Селекторы CSS:

| Элемент | Селектор |
|---------|----------|
| Карточка авто | `[data-testid='vehicle-card']` |
| Заголовок | `.js-title-link` |
| Цена | `[data-testid='price-primary']` |
| Детали | `.rk-preset` |
| Локация | `[data-testid='seller-location']` |

---

## CarsComParser (США)

### Особенности:
- Десктопная версия www.cars.com
- Селекторы: `[data-cy='vehicleCard']`
- Автоматическое закрытие cookie баннера
- Парсинг: цена, год, пробег, двигатель, привод

### Пример использования:

```csharp
var parser = new CarsComParser(browser, logger);

// Поиск Audi A3 в США (радиус 99999 миль от NYC)
var searchUrl = "https://www.cars.com/shopping/audi/a3/?page_size=20&zip=10001&distance=99999";

var result = await parser.ParseSearchPageAsync(searchUrl, pageIndex: 1);

foreach (var car in result.Cars)
{
    Console.WriteLine($"{car.Title} - ${car.Price} USD");
    Console.WriteLine($"  {car.Year}, {car.Mileage} mi, {car.Fuel}, {car.Drivetrain}");
}
```

### Селекторы CSS:

| Элемент | Селектор |
|---------|----------|
| Карточка авто | `[data-cy='vehicleCard']` |
| Заголовок | `[data-cy='vehicleTitle'] a` |
| Цена | `[data-cy='primaryPrice']` |
| Детали | `[data-cy='vehicleDetails']` |
| Локация | `[data-cy='dealerLocation']` |
| Cookie баннер | `[data-testid='cookie-consent-accept-button']` |

---

## Конвертация валют

Все цены конвертируются в RUB через `ICurrencyConversionService`:

- CNY → RUB (Китай)
- EUR → RUB (Европа)
- USD → RUB (США)

---

## Тестирование

### Тест Che168:
```bash
dotnet run --project test-parser.cs
```

### Тест Mobile.de:
```bash
dotnet run --project test-mobile-de.cs
```

### Тест Cars.com:
```bash
dotnet run --project test-cars-com.cs
```

---

## Умная очередь

Worker автоматически определяет регионы из `CarSearchQueue.Regions` (JSON массив):

```json
["china", "europe", "usa"]
```

Приоритеты парсинга:
- 10+ пользователей → каждые 15 мин
- 5-9 пользователей → каждые 30 мин
- 2-4 пользователя → каждые 1 час
- 1 пользователь → каждые 2 часа

---

## Добавление нового парсера

1. Создать DTO: `XxxCarDto.cs`
2. Создать парсер: `XxxParser.cs`
3. Добавить в DI: `AddXxxParser()` в `DependencyInjection.cs`
4. Обновить `CarSearchQueueWorker.ParseRegionAsync()`
5. Обновить `CarStorageService.GetMarketInfoForSource()`

---

**Последнее обновление:** 20 Февраля 2026
