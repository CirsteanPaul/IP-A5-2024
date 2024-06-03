using System.Reflection;
using IP.Project.Entities;
using Microsoft.EntityFrameworkCore;

namespace IP.Project.Database
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<VpnAccount> Vpns { get; set; }
        public virtual DbSet<SambaAccount> SambaAccounts { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Applies entity model configurations from the Data Configuration namespace
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
