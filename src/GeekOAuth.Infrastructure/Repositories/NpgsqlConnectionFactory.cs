using System.Data;
using GeekOAuth.Abstractions.Data;
using Npgsql;

namespace GeekOAuth.Infrastructure.Repositories;

public class NpgsqlConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public IDbConnection CreateConnection() => dataSource.CreateConnection();
}
