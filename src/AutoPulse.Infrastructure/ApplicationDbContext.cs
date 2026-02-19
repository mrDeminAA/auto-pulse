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
    
    // Новые сущности для системы мониторинга
    public DbSet<User> Users => Set<User>();
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();
    public DbSet<UserCar> UserCars => Set<UserCar>();
    public DbSet<CarAlert> CarAlerts => Set<CarAlert>();
    public DbSet<PriceHistory> PriceHistories => Set<PriceHistory>();
    public DbSet<UserSearch> UserSearchs => Set<UserSearch>();
    public DbSet<CarSearchQueue> CarSearchQueues => Set<CarSearchQueue>();
    public DbSet<UserSearchQueue> UserSearchQueues => Set<UserSearchQueue>();

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

        // Конфигурация User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.TelegramId).HasMaxLength(50);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.TelegramId).HasFilter("\"TelegramId\" IS NOT NULL");
        });

        // Конфигурация UserPreferences
        modelBuilder.Entity<UserPreferences>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BodyTypes).HasMaxLength(500);
            entity.Property(e => e.Engines).HasMaxLength(500);
            entity.Property(e => e.Regions).HasMaxLength(500);
            entity.HasOne(e => e.User)
                  .WithOne(u => u.Preferences)
                  .HasForeignKey<UserPreferences>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Конфигурация UserCar
        modelBuilder.Entity<UserCar>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.FavoriteCars)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Car)
                  .WithMany()
                  .HasForeignKey(e => e.CarId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.CarId }).IsUnique();
        });

        // Конфигурация CarAlert
        modelBuilder.Entity<CarAlert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.CarAlerts)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Car)
                  .WithMany()
                  .HasForeignKey(e => e.CarId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Конфигурация PriceHistory
        modelBuilder.Entity<PriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OldCurrency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.NewCurrency).IsRequired().HasMaxLength(3);
            entity.HasOne(e => e.Car)
                  .WithMany()
                  .HasForeignKey(e => e.CarId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.CarId);
        });

        // Конфигурация UserSearch
        modelBuilder.Entity<UserSearch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Generation).HasMaxLength(50);
            entity.Property(e => e.Regions).HasMaxLength(500);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Brand)
                  .WithMany()
                  .HasForeignKey(e => e.BrandId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Model)
                  .WithMany()
                  .HasForeignKey(e => e.ModelId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.UserId);
        });

        // Конфигурация CarSearchQueue
        modelBuilder.Entity<CarSearchQueue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Generation).HasMaxLength(50);
            entity.Property(e => e.Regions).IsRequired().HasMaxLength(500);
            entity.Property(e => e.LastError).HasMaxLength(1000);
            entity.HasOne(e => e.Brand)
                  .WithMany()
                  .HasForeignKey(e => e.BrandId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Model)
                  .WithMany()
                  .HasForeignKey(e => e.ModelId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.BrandId, e.ModelId, e.Generation, e.YearFrom, e.YearTo });
        });

        // Конфигурация UserSearchQueue
        modelBuilder.Entity<UserSearchQueue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.UserSearch)
                  .WithMany(us => us.SearchQueues)
                  .HasForeignKey(e => e.UserSearchId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.CarSearchQueue)
                  .WithMany(cs => cs.UserSearches)
                  .HasForeignKey(e => e.CarSearchQueueId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserSearchId, e.CarSearchQueueId }).IsUnique();
        });
    }
}
