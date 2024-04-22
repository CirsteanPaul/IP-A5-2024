using IP.Project.Database;
using IP.Project.IntegrationTests.Base.AccountSeed;

namespace IP.Project.IntegrationTests.Base;

public class Seed
{
    public static void InitializeDbForTests(ApplicationDBContext context)
    {
        context.AddSambaAccounts();
        context.AddAccounts();
        context.SaveChanges();
    }
}
