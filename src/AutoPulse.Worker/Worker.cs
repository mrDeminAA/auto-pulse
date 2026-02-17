using MassTransit;
using AutoPulse.Infrastructure.Messaging;

namespace AutoPulse.Worker;

/// <summary>
/// Фоновый сервис для демонстрации парсинга
/// </summary>
public class ParserWorker : BackgroundService
{
    private readonly ILogger<ParserWorker> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public ParserWorker(
        ILogger<ParserWorker> logger,
        IPublishEndpoint publishEndpoint,
        IConfiguration configuration)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Parser Worker запущен");

        // Ждём пока MassTransit подключится к RabbitMQ
        await Task.Delay(5000, stoppingToken);

        // Отправляем команду на парсинг (для демонстрации)
        // В реальности это будет приходить через API или по расписанию
        try
        {
            // Пример URL для парсинга (нужно заменить на реальный URL Autohome)
            var chinaUrl = _configuration.GetValue<string>("Parser:ChinaUrl") 
                ?? "https://www.autohome.com.cn/beijing/";

            _logger.LogInformation("Отправка команды на парсинг: {Url}", chinaUrl);

            await _publishEndpoint.Publish(new ParseCarsCommand(
                chinaUrl,
                "Autohome",
                "China"
            ), stoppingToken);

            _logger.LogInformation("Команда на парсинг отправлена");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке команды на парсинг");
        }

        // В реальном приложении здесь будет цикл по расписанию
        // или обработка команд из очереди
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            
            // Можно периодически отправлять команды на парсинг
            // или использовать Hangfire/Quartz для расписания
        }
    }
}
