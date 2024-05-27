namespace IP.Project.Contracts
{
    public class UpdateUserRequest
    {        
        public string Mail { get; set; } = string.Empty;

        public string MailAlternateAddress { get; set; } = string.Empty;

        public string UserPassword { get; set; } = string.Empty;

        public string TelephoneNumber { get; set; } = string.Empty;

    }
}