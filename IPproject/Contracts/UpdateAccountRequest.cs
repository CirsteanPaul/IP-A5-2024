namespace IP.Project.Contracts
{
    public class UpdateAccountRequest
    {
        public string? NewUsername { get; set; }
        public string? NewPassword { get; set; }
        public string? NewEmail { get; set; }
    }
}
