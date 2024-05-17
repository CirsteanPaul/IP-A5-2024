using Microsoft.AspNetCore.Identity;

namespace IP.Project.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
    }
}
