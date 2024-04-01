using IP.VerticalSliceArchitecture.Entities;
using Microsoft.EntityFrameworkCore;

namespace IP.VerticalSliceArchitecture.Database
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : 
            base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }
    }
}
