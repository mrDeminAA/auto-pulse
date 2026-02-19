using Serilog;
using Microsoft.EntityFrameworkCore;
using AutoPulse.Infrastructure;
using AutoPulse.Infrastructure.Repositories;
using AutoPulse.Application.Common.Interfaces;
using AutoPulse.Application.Common.Mappings;
using AutoPulse.Infrastructure.Services;
using MassTransit;
using AutoPulse.Api.Endpoints;
using AutoPulse.Api.Services;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Настройка Serilog
builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
                 .ReadFrom.Services(services));

// Добавление сервисов
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрация DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрация Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "AutoPulse_";
});

// Регистрация MassTransit (RabbitMQ)
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"), h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

// Регистрация сервисов
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

// Регистрация JWT аутентификации
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "")
        )
    };
});

// Регистрация сервиса конвертации валют
builder.Services.AddCurrencyConversion();

// Регистрация AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Регистрация MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(AutoPulse.Application.Markets.Queries.GetAllMarketsQuery).Assembly);
});

// Регистрация FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<AutoPulse.Application.Markets.Queries.GetAllMarketsQuery>();

// Регистрация Unit of Work и Repository Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Health Checks
builder.Services.AddHealthChecks();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Настройка пайплайна
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// API Endpoints
app.MapAuthEndpoints();
app.MapUserSearchEndpoints();
app.MapBrandsEndpoints();
app.MapCarsEndpoints();
app.MapMarketsEndpoints();
app.MapDealersEndpoints();
app.MapParseEndpoints();

// Миграция БД при запуске (для разработки)
// Отключено - применяйте миграции вручную: dotnet ef database update
// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     try
//     {
//         await dbContext.Database.MigrateAsync();
//         Log.Information("Миграция БД выполнена успешно");
//     }
//     catch (Exception ex)
//     {
//         Log.Error(ex, "Ошибка при миграции БД");
//     }
// }

try
{
    Log.Information("Запуск AutoPulse API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложение завершилось аварийно");
}
finally
{
    await Log.CloseAndFlushAsync();
}
