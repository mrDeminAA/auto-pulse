using Microsoft.EntityFrameworkCore;
using MassTransit;
using AutoPulse.Infrastructure;
using AutoPulse.Application.Parsing;
using AutoPulse.Infrastructure.Services;
using AutoPulse.Infrastructure.Messaging;
using AutoPulse.Worker;
using Serilog;

// Bootstrap Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // Add Serilog
    builder.Logging.ClearProviders();
    Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));

    var logConfig = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console();
    
    builder.Services.AddSerilog(logConfig.CreateLogger());

    // Регистрация DbContext
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Регистрация Redis
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = "AutoPulse_Worker_";
    });

    // Регистрация MassTransit с Consumer
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<ParseCarsCommandConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            var rabbitMqHost = builder.Configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@localhost:5672";
            cfg.Host(rabbitMqHost);

            cfg.ReceiveEndpoint("parse-cars-queue", e =>
            {
                e.ConfigureConsumer<ParseCarsCommandConsumer>(context);
            });
        });
    });

    // Регистрация парсера для Китая (Che168) - основной на данный момент
    builder.Services.AddHttpClient<Che168Parser>(client =>
    {
        client.BaseAddress = new Uri("https://www.che168.com");
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
        );
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en;q=0.8");
    });
    builder.Services.AddScoped<ICarParser, Che168Parser>();

    // Регистрация парсера для Autohome (альтернативный)
    builder.Services.AddHttpClient<AutohomeParser>(client =>
    {
        client.BaseAddress = new Uri("https://www.autohome.com.cn");
    });
    // builder.Services.AddScoped<ICarParser, AutohomeParser>(); // Альтернативный парсер

    // Регистрация сервиса обработки данных
    builder.Services.AddScoped<IParsedDataService, ParsedDataService>();

    // Регистрация Worker Health Service
    builder.Services.AddHostedService<WorkerHealthService>();

    var host = builder.Build();

    // Миграция БД при запуске
    using (var scope = host.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            await dbContext.Database.MigrateAsync();
            Log.Information("Миграция БД выполнена успешно (Worker)");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при миграции БД (Worker)");
        }
    }

    Log.Information("Запуск AutoPulse Worker");
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker завершился аварийно");
}
finally
{
    await Log.CloseAndFlushAsync();
}
