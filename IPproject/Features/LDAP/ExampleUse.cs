using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IP.Project.Features.LDAP
{

    [Authorize(Roles = "managers")]
    public class AdminController : Controller
    {
        // ...
    }
}
