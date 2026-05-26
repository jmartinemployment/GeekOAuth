using System.Data;

namespace GeekOAuth.Abstractions.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
