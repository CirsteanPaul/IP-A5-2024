using System.Data.SQLite;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using Dapper;
using IP.Project.Constants;
using IP.Project.Database;
using IP.Project.IntegrationTests.Base.AccountSeed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace IP.Project.IntegrationTests.Base
{
    public class TestingBaseWebApplicationFactory : IDisposable
    {
        private readonly WebApplicationFactory<Program> applicationFactory;

        public HttpClient Client { get; }
        private string dbPath { get; set; }
        private string connectionString { get; set; }
        
        public TestingBaseWebApplicationFactory()
        {
            applicationFactory = new WebApplicationFactory<Program>();
            applicationFactory = applicationFactory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                // This line will stop the auto-migration from the Program.cs configuration
                // It is required because the InMemoryDatabase has no Migrate function
              
                builder.ConfigureServices(services =>
                {
                    var dbName = $"{Guid.NewGuid()}.sqlite"; // For each test class we create a different
                    // database so we don't have date races between threads
                    var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    dbPath = Path.Join(currentDirectory, dbName);
                    SQLiteConnection.CreateFile(dbPath);
                    SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
                    connectionString = $"Data Source={dbPath};";
                    services.RemoveAll(typeof(DbContextOptions<ApplicationDBContext>));
                    services.RemoveAll(typeof(ApplicationDBContext));
                    services.AddDbContext<ApplicationDBContext>(options =>
                    {
                        options.UseSqlite(connectionString);
                    });
                    services.Configure<JwtBearerOptions>(
                        JwtBearerDefaults.AuthenticationScheme,
                        options =>
                        {
                            options.Configuration = new OpenIdConnectConfiguration
                            {
                                Issuer = JwtTokenProvider.Issuer,
                            };
                            options.TokenValidationParameters.ValidIssuer = JwtTokenProvider.Issuer;
                            options.TokenValidationParameters.ValidAudience = JwtTokenProvider.Issuer;
                            options.Configuration.SigningKeys.Add(JwtTokenProvider.SecurityKey);
                        }
                    );
                    
                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
                      
                        
                        db.Database.EnsureCreated();
        
                        Seed.InitializeDbForTests(db);
                    }
                    var myConfiguration = new Dictionary<string, string>
                    {
                        {"ConnectionStrings:Database", connectionString},
                    };
                    var configuration = new ConfigurationBuilder()
                        .AddInMemoryCollection(myConfiguration)
                        .Build();
                    services.RemoveAll(typeof(ISqlConnectionFactory));
                    services.AddSingleton<ISqlConnectionFactory, SqlLiteFactory>();
                    services.AddSingleton<IConfiguration>(configuration);
                });
                SqlMapper.AddTypeHandler(new DapperSqliteHandlers.GuidHandler());
                SqlMapper.AddTypeHandler(new DapperSqliteHandlers.TimeSpanHandler());
                SqlMapper.AddTypeHandler(new DapperSqliteHandlers.DateTimeOffsetHandler());
                
                    
            });
            Client = applicationFactory.CreateClient();
        }

        public void Dispose()
        {
            var sp = applicationFactory.Services;
            using var scope = sp.CreateScope();
            var services = scope.ServiceProvider;
            var db = services.GetRequiredService<ApplicationDBContext>();
            db.Database.EnsureDeleted();
            File.Delete(dbPath);
            Client.Dispose();
        }
        
        public static string CreateUserToken()
        {

            return JwtTokenProvider.JwtSecurityTokenHandler.WriteToken(
                new JwtSecurityToken(
                    JwtTokenProvider.Issuer,
                    JwtTokenProvider.Issuer,
                    new List<Claim> { new(ClaimTypes.Role, Roles.User) },
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: JwtTokenProvider.SigningCredentials
                ));
        }
        public static string CreateAdminToken()
        {

            return JwtTokenProvider.JwtSecurityTokenHandler.WriteToken(
                new JwtSecurityToken(
                    JwtTokenProvider.Issuer,
                    JwtTokenProvider.Issuer,
                    new List<Claim> { new(ClaimTypes.Role, Roles.Admin) },
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: JwtTokenProvider.SigningCredentials
                ));
        }
    }
}