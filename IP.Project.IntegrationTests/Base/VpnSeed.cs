using IP.Project.Database;
using IP.Project.Entities;

namespace IP.Project.IntegrationTests.Base;

public static class VpnSeed
{
    public static void AddVpns(this ApplicationDBContext context)
    {
        var vpns = new List<VpnAccount>
        {
            CreateVpn("10.0.0.1"),
            CreateVpnSpecialId("10.0.0.2")
        };

        context.AddRange(vpns);
    }

    private static VpnAccount CreateVpn(string ipv4Address, string? description = null) => new VpnAccount()
    {
        Id = Guid.NewGuid(),
        IPv4Address = ipv4Address,
        Description = description
    };

    private static VpnAccount CreateVpnSpecialId(string ipv4Address, string? description = null) => new VpnAccount()
    {
        Id = Guid.Parse("b1f5d163-ff83-411a-4144-08dc5ef3042e"),
        IPv4Address = ipv4Address,
        Description = description
    };
}
