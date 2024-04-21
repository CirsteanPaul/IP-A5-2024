using IP.Project.Database;
using IP.Project.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IP.Project.Tests.Base;

public class BaseTest<TEntity> where TEntity : class
{
    protected Mock<ApplicationDBContext> Setup(IEnumerable<TEntity> dbSet)
    {
        IQueryable<TEntity> entities = new List<TEntity>(dbSet).AsQueryable();
        
        var contextMock = SetupDbSetMock(entities, out var setMock);

        SetupDbMock(contextMock, setMock);        

        return contextMock;
    }

    private static Mock<ApplicationDBContext> SetupDbSetMock(IQueryable<TEntity> entities, out Mock<DbSet<TEntity>> setMock)
    {
        var contextMock = new Mock<ApplicationDBContext>(new DbContextOptions<ApplicationDBContext>());

        setMock = new Mock<DbSet<TEntity>>();

        setMock.As<IAsyncEnumerable<TEntity>>()
            .Setup(m => m.GetAsyncEnumerator(default))
            .Returns(new AsyncHelper.TestAsyncEnumerator<TEntity>(entities.GetEnumerator()));

        setMock.As<IQueryable<TEntity>>()
            .Setup(m => m.Provider)
            .Returns(new AsyncHelper.TestAsyncQueryProvider<TEntity>(entities.Provider));
        setMock.As<IQueryable<TEntity>>().Setup(m => m.Expression)
            .Returns(entities.Expression);
        setMock.As<IQueryable<TEntity>>().Setup(m => m.ElementType)
            .Returns(entities.ElementType);
        setMock.As<IQueryable<TEntity>>().Setup(m => m.GetEnumerator())
            .Returns(() => entities.GetEnumerator());
        return contextMock;
    }

    private void SetupDbMock(Mock<ApplicationDBContext> dbMock, Mock<DbSet<TEntity>> setMock)
    {
        switch (setMock.Object)
        {
            case DbSet<Account> accounts:
                dbMock.Setup(c => c.Accounts).Returns(accounts);
                return;
            case DbSet<SambaAccount> sambas:
                dbMock.Setup(c => c.SambaAccounts).Returns(sambas);
                return;
            case DbSet<Vpn> vpns:
                dbMock.Setup(c => c.Vpns).Returns(vpns);
                return;
            default:
                throw new ArgumentException("Invalid type");
        }
    }
}