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
    
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .WriteTo.Console()
        .CreateLogger();

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
            cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"), h =>
            {
                h.Username("guest");
                h.Password("guest");
            });
            
            cfg.ReceiveEndpoint("parse-cars-queue", e =>
            {
                e.ConfigureConsumer<ParseCarsCommandConsumer>(context);
            });
        });
    });

    // Регистрация парсера для Китая
    builder.Services.AddHttpClient<AutohomeParser>(client =>
    {
        client.BaseAddress = new Uri("https://www.autohome.com.cn");
    });

    // Регистрация сервиса обработки данных
    builder.Services.AddScoped<IParsedDataService, ParsedDataService>();

    // Регистрация Worker
    builder.Services.AddHostedService<ParserWorker>();

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
