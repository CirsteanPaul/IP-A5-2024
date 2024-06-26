using IP.Project.Database;
using IP.Project.Entities;

namespace IP.Project.IntegrationTests.Base;

public static class SambaSeed
{
    public static void AddSambaAccounts(this ApplicationDBContext context)
    {
        var sambaAccounts = new List<SambaAccount>
        {
            CreateSambaAccount("102.100.170.255"),
            CreateSambaAccountSpecialId("102.105.160.255")
        };

        context.SambaAccounts.AddRange(sambaAccounts);
    }
    
    private static SambaAccount CreateSambaAccount(string ip, string? description = null) => new SambaAccount()
    {
        Id = Guid.NewGuid(),
        IPv4Address = ip,
        Description = description
    };
    private static SambaAccount CreateSambaAccountSpecialId(string ip, string? description = null) => new SambaAccount()
    {
        Id = Guid.Parse("b1f5d163-ff83-411a-4144-08dc5ef3042e"),
        IPv4Address = ip,
        Description = description
    };
}