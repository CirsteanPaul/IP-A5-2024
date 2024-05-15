using IP.Project.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IP.Project.Database;

public class IdentityDBContext : IdentityDbContext<ApplicationUser>
{
    public IdentityDBContext(DbContextOptions<IdentityDBContext> options) : base(options)
    {
    }
}