namespace IP.Project.Features.LDAP
{
    public class ConfigurationAD
    {
        public int Port { get; set; } = 10389;
        public string Zone { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string LDAPserver { get; set; } = string.Empty;
        public string LDAPQueryBase { get; set; } = string.Empty;

        public string Crew { get; set; } = string.Empty;
        public string Managers { get; set; } = string.Empty;
    }
}
