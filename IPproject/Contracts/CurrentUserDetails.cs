namespace IP.Project.Contracts;

public class CurrentUserDetails
{
    public bool IsAuthenticated { get; set; }
    public string? UserName { get; set; }
    public Dictionary<string, string>? Claims { get; set; }
}