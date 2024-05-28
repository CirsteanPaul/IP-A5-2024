namespace IP.Project.Contracts.Vpn;

public class UpdateVpnRequest
{
    public string NewIpAddress { get; set; } = string.Empty;
    public string NewDescription { get; set; } = string.Empty;
}