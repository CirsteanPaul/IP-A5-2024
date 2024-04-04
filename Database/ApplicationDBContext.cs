using IP.Project.Entities;
using Microsoft.EntityFrameworkCore;

namespace IP.Project.Database
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : 
            base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Vpn> Vpns { get; set; }
    }
}
