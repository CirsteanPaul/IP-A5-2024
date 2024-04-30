namespace IP.Project.Features.LDAP
{
    public class ADUser
    {
        public Guid ObjectGUID { get; set; }

        public string SAMAccountName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;

        public DateTime WhenCreated { get; set; }

        public List<string> MemberOf { get; set; } = [];
    }
}
