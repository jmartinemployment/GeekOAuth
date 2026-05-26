using Npgsql;

namespace GeekOAuth.Server.Infrastructure;

internal static class DatabaseConnection
{
    public static NpgsqlDataSource CreateDataSource(string databaseUrl)
    {
        var normalized = NormalizeConnectionString(databaseUrl);
        return new NpgsqlDataSourceBuilder(normalized).Build();
    }

    /// <summary>
    /// Railway and other hosts provide postgresql:// URIs; NpgsqlDataSourceBuilder expects keyword format.
    /// </summary>
    internal static string NormalizeConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("DATABASE_URL is empty.");

        if (!connectionString.Contains("://", StringComparison.Ordinal))
            return connectionString;

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);

        var sslMode = uri.Host.Contains(".railway.internal", StringComparison.Ordinal)
            ? SslMode.Disable
            : SslMode.Require;

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            SslMode = sslMode,
        };

        if (userInfo.Length > 1)
            builder.Password = Uri.UnescapeDataString(userInfo[1]);

        return builder.ConnectionString;
    }
}
