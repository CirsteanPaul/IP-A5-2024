using IP.Project.Database;
using IP.Project.Entities;

namespace IP.Project.IntegrationTests.Base.AccountSeed;

public static class AccountSeed
{
    public static void AddAccounts(this ApplicationDBContext context)
    {
        var accounts = new List<Account>
        {
            CreateAccount("testAccount1", "testPassword1", "testEmail1", "testMatricol1"),
            CreateAccountSpecialId("testAccount2", "testPassword2", "testEmail2", "testMatricol2"),
        };

        context.Accounts.AddRange(accounts);
    }

    private static Account CreateAccount(string username, string password, string email, string matricol) => new Account()
    {
        Id = Guid.NewGuid(),
        Username = username,
        Password = password,
        Email = email,
        Matricol = matricol,
        CreatedOnUtc = DateTime.UtcNow,
        LastUpdatedOnUtc = DateTime.UtcNow
    };

    private static Account CreateAccountSpecialId(string username, string password, string email, string matricol) => new Account()
    {
        Id = Guid.Parse("b5ec0aed-e1f4-4115-80e4-e4448b1f43ab"),
        Username = username,
        Password = password,
        Email = email,
        Matricol = matricol,
        CreatedOnUtc = DateTime.UtcNow,
        LastUpdatedOnUtc = DateTime.UtcNow
    };
}
