using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IP.Project.LDAP
{

    [Authorize(Roles = "managers")]
    public class AdminController : Controller
    {
        // ...
    }
}
