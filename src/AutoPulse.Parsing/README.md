# AutoPulse.Parsing

Библиотека парсеров для сайтов с автомобилями.

## Che168 Parser

Парсер для китайского сайта Che168.com через прямое API.

### Установка

```bash
dotnet add reference src/AutoPulse.Parsing/AutoPulse.Parsing.csproj
```

### Регистрация в DI

```csharp
builder.Services.AddParsers();
// или только Che168
builder.Services.AddChe168Parser();
```

### Использование

```csharp
public class MyService
{
    private readonly Che168ApiParser _parser;
    
    public MyService(Che168ApiParser parser)
    {
        _parser = parser;
    }
    
    public async Task ParseCarsAsync()
    {
        // Получить одну страницу
        var cars = await _parser.ParseAsync(
            brandId: Che168ApiParser.BrandIds.Audi,
            pageIndex: 1,
            pageSize: 10
        );
        
        // Или получить все автомобили с пагинацией
        await foreach (var car in _parser.ParseAllAsync(
            brandId: Che168ApiParser.BrandIds.Audi,
            maxPages: 100))
        {
            Console.WriteLine($"{car.Brand} {car.Model} - {car.Price} ¥");
        }
    }
}
```

### Доступные бренды

```csharp
Che168ApiParser.BrandIds.Audi        // "33"
Che168ApiParser.BrandIds.Bmw         // "56"
Che168ApiParser.BrandIds.Mercedes    // "57"
```

### Настройки (appsettings.json)

```json
{
  "Che168": {
    "BrandId": "33",
    "MaxPages": 100
  }
}
```

### Структура данных

```csharp
public class ParsedCarData
{
    public long Id { get; set; }           // ID объявления
    public string Brand { get; set; }      // Бренд (奥迪)
    public string Model { get; set; }      // Модель (A4L)
    public string FullName { get; set; }   // Полное название
    public int Year { get; set; }          // Год
    public decimal Price { get; set; }     // Цена в ¥
    public int Mileage { get; set; }       // Пробег в км
    public string City { get; set; }       // Город
    public string Dealer { get; set; }     // Дилер
    public string? Displacement { get; set; } // Двигатель
    public string? ImageUrl { get; set; }  // URL изображения
    public string SourceUrl { get; set; }  // URL источника
    public DateTime ParsedAt { get; set; } // Дата парсинга
    public string Source { get; set; }     // Источник (Che168)
}
```

### Worker Service

Для автоматического парсинга и записи в БД используется `CarParserWorker`:

```csharp
// Program.cs
builder.Services.AddHostedService<CarParserWorker>();
builder.Services.AddSingleton<ICarParseQueue, CarParseQueue>();
builder.Services.AddScoped<ICarStorageService, CarStorageService>();
```

Worker:
1. Запускает парсинг по расписанию
2. Добавляет автомобили в очередь (`ICarParseQueue`)
3. Обрабатывает очередь и сохраняет в БД (`ICarStorageService`)

### Архитектура

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│ Che168ApiParser │────▶│ ICarParseQueue   │────▶│ ICarStorageService│
│                 │     │ (Channel<T>)     │     │ (DbContext)     │
└─────────────────┘     └──────────────────┘     └─────────────────┘
```

### Примечания

- API Che168 требует мобильные заголовки (iPhone User-Agent)
- Цена возвращается в юанях (умножается на 10000 из 万)
- Пробег возвращается в км (умножается на 10000 из 万 км)
- Рекомендуется делать задержку между запросами (реализовано в `ParseAllAsync`)
