using Microsoft.Extensions.Options;

namespace IP.Project.Features.Accounts;

public class LdapService(IOptions<LdapSettings> ldapSettings) : ILdapService
{
    private readonly LdapSettings LdapSettings = ldapSettings.Value;
    public LdapSettings GetLdapSettings() => LdapSettings;
}
