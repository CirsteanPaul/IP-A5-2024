using IP.Project.Database;

namespace IP.Project.IntegrationTests.Base;

public class Seed
{
    public static void InitializeDbForTests(ApplicationDBContext context)
    {
        context.AddSambaAccounts();
        context.SaveChanges();
    }
}
