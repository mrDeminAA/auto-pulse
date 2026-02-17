using MassTransit;
using AutoPulse.Application.Parsing;
using AutoPulse.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Infrastructure.Messaging;

public class ParseCarsCommandConsumer : IConsumer<ParseCarsCommand>
{
    private readonly ICarParser _parser;
    private readonly IParsedDataService _dataService;
    private readonly ILogger<ParseCarsCommandConsumer> _logger;

    public ParseCarsCommandConsumer(
        ICarParser parser,
        IParsedDataService dataService,
        ILogger<ParseCarsCommandConsumer> logger)
    {
        _parser = parser;
        _dataService = dataService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ParseCarsCommand> context)
    {
        var command = context.Message;
        
        _logger.LogInformation("Начало парсинга {Source} для {Country} по URL: {Url}", 
            command.SourceName, command.Country, command.Url);

        try
        {
            // Парсим список автомобилей
            var parsedCars = await _parser.ParseListAsync(command.Url, context.CancellationToken);

            _logger.LogInformation("Спаршено {Count} автомобилей", parsedCars.Count);

            // Обрабатываем и сохраняем каждый автомобиль
            var processedCars = new List<ParsedCarData>();
            foreach (var parsedCar in parsedCars)
            {
                try
                {
                    await _dataService.ProcessParsedCarAsync(parsedCar, context.CancellationToken);
                    processedCars.Add(parsedCar);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при сохранении автомобиля {Url}", parsedCar.SourceUrl);
                }
            }

            // Публикуем событие об успешном парсинге
            await context.Publish(new CarsParsedEvent(
                command.SourceName,
                processedCars.Count,
                DateTime.UtcNow,
                processedCars
            ));

            _logger.LogInformation("Парсинг завершён. Обработано {Count} из {Total}", 
                processedCars.Count, parsedCars.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критическая ошибка при парсинге {Url}", command.Url);
            throw;
        }
    }
}
