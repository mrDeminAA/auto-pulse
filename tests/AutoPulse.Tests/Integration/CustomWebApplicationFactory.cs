using AutoPulse.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Tests.Integration;

/// <summary>
/// Фабрика для интеграционных тестов API
/// Примечание: Требуется совместимая версия Swashbuckle для .NET 11
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Удаляем существующий DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Добавляем in-memory базу данных для тестов
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Build ServiceProvider для инициализации БД
            var sp = services.BuildServiceProvider();

            // Создаём scope и инициализируем БД
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

            try
            {
                // Гарантируем создание БД
                db.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при создании БД: {Message}", ex.Message);
            }
        });

        builder.UseEnvironment("Development");
    }
}
