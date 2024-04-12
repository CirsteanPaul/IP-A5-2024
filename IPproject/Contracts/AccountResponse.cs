namespace IP.Project.Contracts
{
    public class AccountResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Matricol { get; set; } = string.Empty;
        public DateTime CreatedOnUtc { get; set; }

        public DateTime LastUpdatedOnUtc { get; set; }
    }
}