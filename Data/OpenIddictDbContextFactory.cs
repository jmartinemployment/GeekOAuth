using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GeekOAuth.Data;

public class OpenIddictDbContextFactory : IDesignTimeDbContextFactory<OpenIddictDbContext>
{
    public OpenIddictDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? "Host=localhost;Database=geek_oauth_dev;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<OpenIddictDbContext>()
            .UseNpgsql(connectionString)
            .UseOpenIddict()
            .Options;

        return new OpenIddictDbContext(options);
    }
}
