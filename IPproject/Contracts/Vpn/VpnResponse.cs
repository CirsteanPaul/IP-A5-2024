namespace IP.Project.Contracts.Vpn;

public class VpnResponse
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public string IPv4Address { get; set; } = string.Empty;
}