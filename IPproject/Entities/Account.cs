namespace IP.Project.Entities
{
    public class Account
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Matricol {  get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string CNP { get; set; } = string.Empty;
        public string cn { get; set; } = string.Empty;
        public string sn { get; set; } = string.Empty;
        public int gidNumber { get; set; } = 0; // same value as uidNumber
        public int uidNumber { get; set; } = 0; // same value as gidNumber

        public string uid { get; set; } = string.Empty;

        public string homeDirectory { get; set; } = string.Empty;

        public string displayName { get; set; } = string.Empty;

        public string employeeNumber { get; set; } = string.Empty;

        public string givenName { get; set; } = string.Empty;

        public string homePhone { get; set; } = string.Empty;

        public string initials { get; set; } = string.Empty;

        public string localityName { get; set; } = string.Empty;

        public string mail { get; set; } = string.Empty;

        public string mailAlternateAddress { get; set; } = string.Empty;

        public string mobile { get; set; } = string.Empty;

        public string ou { get; set; } = string.Empty; 

        public string postalCode { get; set; } = string.Empty;

        public string roomNumber { get; set; } = string.Empty;

        public string shadowInactive { get; set; } = string.Empty;

        public string street { get; set; } = string.Empty;

        public string telephoneNumber { get; set; } = string.Empty;

        public string title { get; set; } = string.Empty;

        public string userPassword { get; set; } = string.Empty;

        public string description { get; set; } = string.Empty;

        public string mailVariant1 { get; set; } = string.Empty;

        public string mailVariant2 { get; set; } = string.Empty;

        public string mailVariant3 { get; set; } = string.Empty;

        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedOnUtc { get; set; } = DateTime.UtcNow;
    }
}
