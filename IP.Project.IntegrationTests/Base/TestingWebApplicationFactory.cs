using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IP.Project.Database;
using IP.Project.IntegrationTests.Base;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Store.FunctionalTests
{
    public class TestingBaseWebApplicationFactory : IDisposable
    {
        private readonly WebApplicationFactory<Program> applicationFactory;

        public HttpClient Client { get; }
        
        public TestingBaseWebApplicationFactory()
        {
            applicationFactory = new WebApplicationFactory<Program>();
            applicationFactory = applicationFactory.WithWebHostBuilder(builder =>
            {
                // This line will stop the auto-migration from the Program.cs configuration
                // It is required because the InMemoryDatabase has no Migrate function
                builder.UseEnvironment("Testing"); 
                builder.ConfigureServices(services =>
                {
                    var guid = Guid.NewGuid(); // For each test class we create a different
                    // database so we don't have date races between threads
                    services.RemoveAll(typeof(DbContextOptions<ApplicationDBContext>));
                    services.RemoveAll(typeof(ApplicationDBContext));
                    services.AddDbContext<ApplicationDBContext>(options =>
                    {
                        options.UseInMemoryDatabase(guid.ToString());
                    });
                    
                    var sp = services.BuildServiceProvider();
                    using (var scope = sp.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
                        
                        db.Database.EnsureCreated();
        
                        Seed.InitializeDbForTests(db);
                    }
                });
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
            Client.Dispose();
        }
    }
}