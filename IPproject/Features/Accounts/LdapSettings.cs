namespace IP.Project.Features.Accounts
{
    public class LdapSettings
    {
        public string LdapServer { get; set; } = string.Empty;
        public int LdapPort { get; set; } = 0;
        public string AdminUserName { get; set; } = string.Empty;
        public string AdminPassword { get; set; } = string.Empty;
        public string BaseDN { get; set; } = string.Empty;
    }
}
