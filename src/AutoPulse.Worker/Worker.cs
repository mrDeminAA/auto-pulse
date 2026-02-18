using Microsoft.Extensions.Logging;

namespace AutoPulse.Worker;

/// <summary>
/// Фоновый сервис для отслеживания состояния Worker
/// </summary>
public class WorkerHealthService : BackgroundService
{
    private readonly ILogger<WorkerHealthService> _logger;

    public WorkerHealthService(ILogger<WorkerHealthService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker Health Service запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker работает, ожидание сообщений...");
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
