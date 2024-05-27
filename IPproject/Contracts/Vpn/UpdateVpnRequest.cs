namespace IP.Project.Contracts.Vpn;

public class UpdateVpnRequest
{
    public string NewIpAddress { get; set; }
    public string? NewDescription { get; set; }
}