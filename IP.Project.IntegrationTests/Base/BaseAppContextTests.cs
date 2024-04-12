using IP.Project.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IP.Project.IntegrationTests.Base;

public abstract class BaseAppContextTests : IAsyncDisposable
{
    protected readonly WebApplicationFactory<Program> Application;
    protected readonly HttpClient Client;

    protected BaseAppContextTests()
    {
        Application = new WebApplicationFactory<Program>();
        Application = Application.WithWebHostBuilder(builder =>
        {
            // This line will stop the auto-migration from the Program.cs configuration
            // It is required because the InMemoryDatabase has no Migrate function
            builder.UseEnvironment("Testing"); 
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<ApplicationDBContext>));
                services.AddDbContext<ApplicationDBContext>(options =>
                {
                    options.UseInMemoryDatabase("IpProjectDbForTesting");
                });
                
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDBContext>();

                    db.Database.EnsureCreated();

                    Seed.InitializeDbForTests(db);
                }
            });
        });
        Client = Application.CreateClient();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await Application.DisposeAsync();
    }
}
