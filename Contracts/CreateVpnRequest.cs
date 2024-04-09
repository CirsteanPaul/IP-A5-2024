namespace IP.Project.Contracts;

public class CreateVpnRequest
{
    public string? Description { get; set; }
    public string IPv4Address { get; set; } = string.Empty;
}