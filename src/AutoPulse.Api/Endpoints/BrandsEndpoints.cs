using Microsoft.EntityFrameworkCore;
using AutoPulse.Domain;
using AutoPulse.Infrastructure;

namespace AutoPulse.Api.Endpoints;

/// <summary>
/// API для работы с брендами
/// </summary>
public static class BrandsEndpoints
{
    public static void MapBrandsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/brands")
            .WithTags("Brands");

        // GET /api/brands
        group.MapGet("/", async (ApplicationDbContext db, CancellationToken ct) =>
        {
            var brands = await db.Brands
                .AsNoTracking()
                .OrderBy(b => b.Name)
                .Select(b => new BrandDto(
                    b.Id,
                    b.Name,
                    b.Country,
                    b.LogoUrl,
                    b.CreatedAt
                ))
                .ToListAsync(ct);

            return Results.Ok(brands);
        })
        .WithName("GetAllBrands")
        .WithSummary("Получить список всех брендов")
        .Produces<List<BrandDto>>(StatusCodes.Status200OK);

        // POST /api/brands/seed - добавить seed данные (только для разработки)
        group.MapPost("/seed", async (ApplicationDbContext db, CancellationToken ct) =>
        {
            // Проверяем есть ли уже данные
            if (await db.Brands.AnyAsync(ct))
                return Results.Ok(new { message = "Данные уже существуют" });

            // Добавляем бренды
            var brands = new[]
            {
                new Brand("Audi", "Germany"),
                new Brand("BMW", "Germany"),
                new Brand("Mercedes-Benz", "Germany"),
                new Brand("Volkswagen", "Germany"),
                new Brand("Toyota", "Japan"),
                new Brand("Honda", "Japan"),
                new Brand("Nissan", "Japan"),
                new Brand("Mazda", "Japan"),
                new Brand("Lexus", "Japan"),
                new Brand("Porsche", "Germany")
            };
            db.Brands.AddRange(brands);
            await db.SaveChangesAsync(ct);

            // Добавляем модели для Audi (ID=1)
            var audiModels = new[]
            {
                new Model(1, "A3", "Compact"),
                new Model(1, "A4", "Mid-size"),
                new Model(1, "A4L", "Mid-size LWB"),
                new Model(1, "A6", "Executive"),
                new Model(1, "A6L", "Executive LWB"),
                new Model(1, "Q5", "Compact SUV"),
                new Model(1, "Q7", "Full-size SUV"),
                new Model(1, "A5", "Compact Executive"),
                new Model(1, "Q3", "Subcompact SUV"),
                new Model(1, "e-tron", "Electric SUV")
            };
            db.Models.AddRange(audiModels);

            // Добавляем модели для BMW (ID=2)
            var bmwModels = new[]
            {
                new Model(2, "3 Series", "Compact Executive"),
                new Model(2, "5 Series", "Executive"),
                new Model(2, "7 Series", "Full-size Luxury"),
                new Model(2, "X3", "Compact SUV"),
                new Model(2, "X5", "Mid-size SUV"),
                new Model(2, "X7", "Full-size SUV")
            };
            db.Models.AddRange(bmwModels);

            await db.SaveChangesAsync(ct);

            return Results.Ok(new { message = "Seed данные добавлены", brands = brands.Length });
        })
        .WithName("SeedBrands")
        .WithSummary("Добавить seed данные (Dev)")
        .Produces(StatusCodes.Status200OK);

        // GET /api/brands/{id}
        group.MapGet("/{id:int}", async (int id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var brand = await db.Brands
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id, ct);

            if (brand == null)
                return Results.NotFound();

            return Results.Ok(new BrandDto(
                brand.Id,
                brand.Name,
                brand.Country,
                brand.LogoUrl,
                brand.CreatedAt
            ));
        })
        .WithName("GetBrandById")
        .WithSummary("Получить бренд по ID")
        .Produces<BrandDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/brands/{id}/models
        group.MapGet("/{id:int}/models", async (int id, ApplicationDbContext db, CancellationToken ct) =>
        {
            var brand = await db.Brands
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id, ct);

            if (brand == null)
                return Results.NotFound();

            var models = await db.Models
                .AsNoTracking()
                .Where(m => m.BrandId == id)
                .OrderBy(m => m.Name)
                .Select(m => new ModelDto(
                    m.Id,
                    m.BrandId,
                    m.Name,
                    m.Category,
                    m.CreatedAt
                ))
                .ToListAsync(ct);

            return Results.Ok(models);
        })
        .WithName("GetBrandModels")
        .WithSummary("Получить модели бренда")
        .Produces<List<ModelDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }

    public record BrandDto(
        int Id,
        string Name,
        string? Country,
        string? LogoUrl,
        DateTime CreatedAt
    );

    public record ModelDto(
        int Id,
        int BrandId,
        string Name,
        string? Category,
        DateTime CreatedAt
    );
}
