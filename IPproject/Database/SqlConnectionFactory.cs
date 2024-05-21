using System.Data;
using Microsoft.Data.SqlClient;

namespace IP.Project.Database;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly IConfiguration configuration;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        this.configuration = configuration;
    }
    
    public SqlConnectionFactory()
    {
    }

    public virtual IDbConnection CreateConnection()
    {
        return new SqlConnection(configuration.GetConnectionString("Database"));
    }
}