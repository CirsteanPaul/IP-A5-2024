namespace IP.Project.Entities
{
    public class Account
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Matricol {  get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdatedOnUtc { get; set; } = DateTime.UtcNow;

        //TODO password
    }
}
