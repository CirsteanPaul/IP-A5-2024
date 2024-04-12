using IP.Project.Database;
using Microsoft.EntityFrameworkCore;

namespace IP.Project.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this WebApplication application) 
        {
            using var scope = application.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>(); 
            dbContext.Database.Migrate();
        }
    }
}
