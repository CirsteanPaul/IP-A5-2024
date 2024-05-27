using System.Data;
using IP.Project.Database;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace IP.Project.IntegrationTests.Base;

public class SqlLiteFactory : ISqlConnectionFactory
{
    private readonly IConfiguration configuration;

    public SqlLiteFactory(IConfiguration configuration)
    {
        this.configuration = configuration;
    }
    
    public SqlLiteFactory()
    {
    }

    public virtual IDbConnection CreateConnection()
    {
        return new SqliteConnection(configuration.GetConnectionString("Database"));
    }
}