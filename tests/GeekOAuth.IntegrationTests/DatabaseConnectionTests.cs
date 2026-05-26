using GeekOAuth.Server.Infrastructure;

namespace GeekOAuth.IntegrationTests;

public class DatabaseConnectionTests
{
    [Fact]
    public void NormalizeConnectionString_ConvertsPostgresqlUri()
    {
        const string uri = "postgresql://postgres:secret@postgres.railway.internal:5432/railway";

        var normalized = DatabaseConnection.NormalizeConnectionString(uri);

        Assert.Contains("Host=postgres.railway.internal", normalized);
        Assert.Contains("Username=postgres", normalized);
        Assert.Contains("Password=secret", normalized);
        Assert.Contains("Database=railway", normalized);
    }

    [Fact]
    public void NormalizeConnectionString_PassesThroughKeywordFormat()
    {
        const string keyword = "Host=localhost;Database=test;Username=u;Password=p";

        Assert.Equal(keyword, DatabaseConnection.NormalizeConnectionString(keyword));
    }
}
