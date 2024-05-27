namespace IP.Project.Contracts.Auth;

public class CurrentUserDetails
{
    public bool IsAuthenticated { get; set; }
    public string? UserName { get; set; }
    public Dictionary<string, string>? Claims { get; set; }
}