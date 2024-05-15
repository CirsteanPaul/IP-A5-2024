using System.Data;

namespace IP.Project.Database;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}