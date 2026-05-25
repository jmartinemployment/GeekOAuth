using System.Data;
using Npgsql;

namespace GeekOAuth.Repositories;

public class NpgsqlConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public IDbConnection CreateConnection() => dataSource.CreateConnection();
}
