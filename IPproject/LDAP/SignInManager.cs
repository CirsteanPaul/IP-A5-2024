using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.DirectoryServices.Protocols;
using System.Security.Claims;
using System.Text;

namespace IP.Project.LDAP
{
    public interface ISignInManager
    {
        Task<bool> SignIn(string username, string password);
        Task SignOut();
    }

    // and then implement it
    public class SignInManager(
        IOptions<ConfigurationAD> configurationAD,
        IHttpContextAccessor httpContextAccessor
        ) : ISignInManager
    {
        private readonly ConfigurationAD _configurationAD = configurationAD.Value;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<bool> SignIn(string username, string password)
        {
            _ = new ADUser();

            var searchResults = General.SearchInAD(
                _configurationAD.LDAPserver,
                _configurationAD.Port,
                _configurationAD.Domain,
                username,
                password,
                $"CN=Users,{_configurationAD.LDAPQueryBase}",
                new StringBuilder("(&")
                    .Append("(objectCategory=person)")
                    .Append("(objectClass=user)")
                    .Append($"(memberOf={_configurationAD.Crew})")
                    .Append("(!(userAccountControl:1.2.840.113556.1.4.803:=2))")
                    .Append($"(sAMAccountName={username})")
                    .Append(')')
                    .ToString(),
                SearchScope.Subtree,
                [
                "objectGUID",
                "sAMAccountName",
                "displayName",
                "mail",
                "whenCreated",
                "memberOf"
                ]
            );

            var results = searchResults.Entries.Cast<SearchResultEntry>();
            ADUser? adUser;
            if (results.Any())
            {
                var resultsEntry = results.First();
                adUser = new ADUser()
                {
                    ObjectGUID = new Guid((resultsEntry.Attributes["objectGUID"][0] as byte[])!),
                    SAMAccountName = resultsEntry.Attributes["sAMAccountName"][0].ToString()!,
                    DisplayName = resultsEntry.Attributes["displayName"][0].ToString()!,
                    Mail = resultsEntry.Attributes["mail"][0].ToString()!,
                    WhenCreated = DateTime.ParseExact(
                        resultsEntry.Attributes["whenCreated"][0].ToString()!,
                        "yyyyMMddHHmmss.0Z",
                        System.Globalization.CultureInfo.InvariantCulture
                    )
                };
                var groups = resultsEntry.Attributes["memberOf"];
                foreach (var g in groups)
                {
                    if (g is byte[] groupNameBytes)
                    {
                        adUser.MemberOf.Add(Encoding.Default.GetString(groupNameBytes).ToLower());
                    }
                }
            }
            else
            {
                Console.WriteLine(
                    $"There is no such user in the [crew] group: {username}"
                );
                return false;
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, adUser.ObjectGUID.ToString()),
                new(ClaimTypes.WindowsAccountName, adUser.SAMAccountName),
                new(ClaimTypes.Name, adUser.DisplayName),
                new(ClaimTypes.Email, adUser.Mail),
                new("whenCreated", adUser.WhenCreated.ToString("yyyy-MM-dd"))
            };
            // perhaps it should add a role for every group, but we only need one for now
            if (adUser.MemberOf.Contains(_configurationAD.Managers.ToLower()))
            {
                claims.Add(new Claim(ClaimTypes.Role, "managers"));
            }

            var identity = new ClaimsIdentity(
                claims,
                "LDAP", // what goes to User.Identity.AuthenticationType
                ClaimTypes.Name, // which claim is for storing user name in User.Identity.Name
                ClaimTypes.Role // which claim is for storing user roles, needed for User.IsInRole()
            );
            var principal = new ClaimsPrincipal(identity);

            if (_httpContextAccessor.HttpContext != null)
            {
                try
                {
                    await _httpContextAccessor.HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal
                    );
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Signing in has failed. {ex.Message}");
                }
            }

            return false;
        }

        public async Task SignOut()
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                await _httpContextAccessor.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme
                );
            }
            else
            {
                throw new Exception(
                    "For some reasons, HTTP context is null, signing out cannot be performed"
                );
            }
        }

        public void SearchExample()
        {

            var attributesToQuery = new string[]
            {
                "objectGUID",
                "sAMAccountName",
                "displayName",
                "mail",
                "whenCreated"
            };
            var searchResults = General.SearchInAD(
                _configurationAD.LDAPserver,
                _configurationAD.Port,
                _configurationAD.Domain,
                _configurationAD.Username,
                _configurationAD.Password,
                $"CN=Users,{_configurationAD.LDAPQueryBase}",
                new StringBuilder("(&")
                    .Append("(objectCategory=person)")
                    .Append("(objectClass=user)")
                    .Append($"(memberOf={_configurationAD.Crew})")
                    .Append("(!(userAccountControl:1.2.840.113556.1.4.803:=2))")
                    .Append(')')
                    .ToString(),
                SearchScope.Subtree,
                attributesToQuery
            );

            foreach (var searchEntry in searchResults.Entries.Cast<SearchResultEntry>())
            {
                foreach (var attr in attributesToQuery)
                {
                    if (searchEntry.Attributes[attr][0].GetType() != typeof(byte[]))
                    {
                        var attrValue = searchEntry.Attributes[attr][0].ToString();
                        Console.WriteLine(attrValue);
                    }
                    else // must be bytes then
                    {
                        if (searchEntry.Attributes[attr][0] is byte[] attrValue)
                        {
                            // can't get a normal string out of it like this
                            var gdString = Encoding.Default.GetString(attrValue);
                            // only can compose a bare string representation of those bytes
                            var gdStringBytes = string.Concat(attrValue!.Select(b => b.ToString("X2")));
                            Console.WriteLine(gdStringBytes);
                        }
                    }
                }
            }
        }
    }
}
