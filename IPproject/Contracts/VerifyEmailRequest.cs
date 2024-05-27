namespace IP.Project.Contracts;

public class VerifyEmailRequest
{
    public string UserId { get; set; }
    public string Token { get; set; }
}
