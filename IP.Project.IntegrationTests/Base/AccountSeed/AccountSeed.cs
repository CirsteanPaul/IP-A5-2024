using IP.Project.Database;
using IP.Project.Entities;

namespace IP.Project.IntegrationTests.Base.AccountSeed;

public static class AccountSeed
{
    public static void AddAccounts(this ApplicationDBContext context)
    {
        var accounts = new List<Account>
        {
            CreateAccountSpecialId("testAccount2", "testPassword2", "testEmail2", "testMatricol2"),
        };

        context.Accounts.AddRange(accounts);
    }

    private static Account CreateAccountSpecialId(string username, string password, string email, string matricol) => new Account()
    {
        Id = Guid.Parse("f1302aae-193b-4ae0-8d1d-b8a3f239fff1"),
        Username = username,
        Password = password,
        Email = email,
        Matricol = matricol,
        CreatedOnUtc = DateTime.UtcNow,
        LastUpdatedOnUtc = DateTime.UtcNow
    };
}
