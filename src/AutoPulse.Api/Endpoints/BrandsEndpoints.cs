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
            .WithTags("Brands")
            .WithOpenApi();

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
