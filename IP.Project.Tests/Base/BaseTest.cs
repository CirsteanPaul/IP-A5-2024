using System.Data;
using IP.Project.Database;
using IP.Project.Entities;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace IP.Project.Tests.Base;

public class BaseTest<TEntity> where TEntity : class
{
    protected ApplicationDBContext Setup(IEnumerable<TEntity> dbSet)
    {
        IQueryable<TEntity> entities = new List<TEntity>(dbSet).AsQueryable();
        
        var contextMock = SetupDbSetMock(entities, out var setMock);

        SetupDbMock(contextMock, setMock);        

        return contextMock;
    }

    
    /**
     * <summary>Method which should be used when dapper is needed in tests.</summary>
     * <example>
     * var sqlConnection = Substiture.For&lt;IDbConnection&gt;().SetupCommands()
     * Your configurations...
     * <code>
     * return sqlConnection
     * </code>
     * </example>
     * <returns>A new factory which will be injected in the handlers using Dapper</returns>
     */
    protected ISqlConnectionFactory SetupDapper(Func<IDbConnection> func)
    {
        var factory = Substitute.For<ISqlConnectionFactory>();

        var connection = func();

        factory.CreateConnection().Returns(connection);
        
        return factory;
    }

    private static ApplicationDBContext SetupDbSetMock(IQueryable<TEntity> entities, out DbSet<TEntity> setMock)
    {
        var contextMock = Substitute.For<ApplicationDBContext>(new DbContextOptions<ApplicationDBContext>());

        setMock = Substitute.For<DbSet<TEntity>, IQueryable<TEntity>, IAsyncEnumerable<TEntity>>();
        
        ((IAsyncEnumerable<TEntity>)setMock).GetAsyncEnumerator()
            .Returns(new AsyncHelper.TestAsyncEnumerator<TEntity>(entities.GetEnumerator()));
        ((IQueryable<TEntity>)setMock).Provider.Returns(new AsyncHelper.TestAsyncQueryProvider<TEntity>(entities.Provider));
        ((IQueryable<TEntity>)setMock).Expression.Returns(entities.Expression);
        ((IQueryable<TEntity>)setMock).ElementType.Returns(entities.ElementType);
        ((IQueryable<TEntity>)setMock).GetEnumerator().Returns(entities.GetEnumerator());

        return contextMock;
    }

    private static void SetupDbMock(ApplicationDBContext dbMock, DbSet<TEntity> setMock)
    {
        switch (setMock)
        {
            case DbSet<Account> accounts:
                dbMock.Accounts.Returns(accounts);
                return;
            case DbSet<SambaAccount> sambas:
                dbMock.SambaAccounts.Returns(sambas);
                return;
            case DbSet<VpnAccount> vpns:
                dbMock.Vpns.Returns(vpns);
                return;
            default:
                throw new ArgumentException("Invalid type");
        }
    }
}