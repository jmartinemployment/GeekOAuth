using System.Data;

namespace GeekOAuth.Repositories;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
