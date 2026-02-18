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
            "User ID=postgres;Password=adminsgesYfdkjnfk;Host=10.23.3.172;Port=5432;Database=autopulse;"
        );

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
