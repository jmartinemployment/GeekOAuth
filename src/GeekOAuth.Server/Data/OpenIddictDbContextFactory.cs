using GeekOAuth.Server.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GeekOAuth.Server.Data;

public class OpenIddictDbContextFactory : IDesignTimeDbContextFactory<OpenIddictDbContext>
{
    public OpenIddictDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? "Host=localhost;Database=geek_oauth_dev;Username=postgres;Password=postgres";

        var dataSource = DatabaseConnection.CreateDataSource(connectionString);

        var options = new DbContextOptionsBuilder<OpenIddictDbContext>()
            .UseNpgsql(dataSource)
            .UseOpenIddict()
            .Options;

        return new OpenIddictDbContext(options);
    }
}
