using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Testcontainers.PostgreSql;

namespace GeekOAuth.IntegrationTests;

public sealed class GeekOAuthWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await ApplyIdentitySchemaAsync();
    }

    private async Task ApplyIdentitySchemaAsync()
    {
        var sqlPath = ResolveIdentityTablesSqlPath();
        var sql = await File.ReadAllTextAsync(sqlPath);
        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    private static string ResolveIdentityTablesSqlPath()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Data", "identity_tables.sql"),
            Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "..",
                "src", "GeekOAuth.Server", "Data", "identity_tables.sql")),
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
                return path;
        }

        throw new FileNotFoundException(
            "identity_tables.sql not found. Expected at: " + string.Join(" or ", candidates));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting("DATABASE_URL", _postgres.GetConnectionString());
        builder.UseSetting("ISSUER_URL", "https://auth.test.local");
        builder.UseSetting("CORS_ORIGINS", "https://app.test.local");
        builder.UseSetting("CLIENT_SECRET_GEEKAPI", "integration-test-geekapi-secret-32chars!");
        builder.UseSetting("CLIENT_SECRET_GEEKWEBSITE", "integration-test-website-secret-32chars!");
        builder.UseSetting("DATA_PROTECTION_PURPOSE", "GeekOAuth-IntegrationTests");
    }
}
