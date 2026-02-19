# Che168 Playwright Parser

Парсер автомобилей с мобильного сайта **m.che168.com** с использованием Playwright.

## Особенности

- Использует мобильную версию сайта для лучшего доступа к данным
- Автоматическая загрузка изображений автомобилей
- Умное определение дубликатов
- Поддержка пагинации
- Извлечение данных: название, цена, год, пробег, город, изображение

## Установка

1. Добавьте пакет NuGet:
```bash
dotnet add package Microsoft.Playwright
```

2. Установите браузер Playwright:
```bash
pwsh bin/Debug/net11.0/playwright.ps1 install chromium
```

## Использование

### Через Dependency Injection

В `Program.cs`:
```csharp
builder.Services.AddParsers();
```

### Запуск парсера

Парсер автоматически запускается через `CarParserWorker` при старте приложения.

Настройки в `appsettings.json`:
```json
{
  "Che168": {
    "BrandUrl": "https://www.che168.com/beijing/aodi/a3/",
    "MaxPages": 20,
    "TargetCount": 100
  }
}
```

### Прямой вызов

```csharp
var parser = serviceProvider.GetRequiredService<Che168PlaywrightParser>();

// Распарсить одну страницу
var cars = await parser.ParseSearchPageAsync(brandUrl, pageIndex: 1);

// Распарсить все автомобили с пагинацией
await foreach (var car in parser.ParseAllAsync(brandUrl, maxPages: 20, targetCount: 100))
{
    Console.WriteLine($"{car.Brand} {car.Model} - {car.Price}");
}
```

## Структура данных

`ParsedCarData` содержит:
- `Id` - уникальный идентификатор
- `Brand` - бренд (например, "奥迪")
- `Model` - модель
- `FullName` - полное название
- `Year` - год выпуска
- `Price` - цена в юанях
- `Mileage` - пробег в км
- `City` - город
- `ImageUrl` - URL изображения
- `SourceUrl` - URL источника
- `Source` - источник ("Che168")

## Алгоритм работы

1. Переход на мобильную версию сайта m.che168.com
2. Скроллинг страницы для полной загрузки контента
3. Извлечение текста и изображений
4. Парсинг текста с помощью регулярных выражений
5. Сопоставление изображений с автомобилями
6. Фильтрация дубликатов
7. Сохранение в очередь для обработки

## Отладка

Для отладки установите уровень логирования:
```json
{
  "Logging": {
    "LogLevel": {
      "AutoPulse.Parsing": "Debug"
    }
  }
}
```

## Зависимости

- .NET 11
- Microsoft.Playwright 1.50.0
- Microsoft.Extensions.Logging
