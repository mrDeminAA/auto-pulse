using Microsoft.EntityFrameworkCore;
using AutoPulse.Domain;

namespace AutoPulse.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSet для всех сущностей
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Model> Models => Set<Model>();
    public DbSet<Dealer> Dealers => Set<Dealer>();
    public DbSet<Market> Markets => Set<Market>();
    public DbSet<DataSource> DataSources => Set<DataSource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Конфигурация Brand
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Конфигурация Model
        modelBuilder.Entity<Model>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.HasOne(e => e.Brand)
                  .WithMany(b => b.Models)
                  .HasForeignKey(e => e.BrandId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.BrandId, e.Name }).IsUnique();
        });

        // Конфигурация Market
        modelBuilder.Entity<Market>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Region).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Конфигурация Dealer
        modelBuilder.Entity<Dealer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Rating).HasDefaultValue(0);
            entity.HasOne(e => e.Market)
                  .WithMany()
                  .HasForeignKey(e => e.MarketId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Конфигурация DataSource
        modelBuilder.Entity<DataSource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BaseUrl).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => new { e.Name, e.Country }).IsUnique();
        });

        // Конфигурация Car
        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).IsRequired();
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.SourceUrl).IsRequired();
            entity.Property(e => e.Vin).HasMaxLength(17);
            
            entity.HasOne(e => e.Brand)
                  .WithMany()
                  .HasForeignKey(e => e.BrandId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Model)
                  .WithMany(m => m.Cars)
                  .HasForeignKey(e => e.ModelId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Market)
                  .WithMany(m => m.Cars)
                  .HasForeignKey(e => e.MarketId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Dealer)
                  .WithMany(d => d.Cars)
                  .HasForeignKey(e => e.DealerId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.DataSource)
                  .WithMany()
                  .HasForeignKey(e => e.DataSourceId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Vin).HasFilter("\"Vin\" IS NOT NULL");
            entity.HasIndex(e => e.SourceUrl).IsUnique();
        });
    }
}
