using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AutoPulse.Infrastructure;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Connection string для миграций (разработка)
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=autopulse;Username=postgres;Password=postgres123"
        );

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
