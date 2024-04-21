using Moq;
using Microsoft.EntityFrameworkCore;
using IP.Project.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using IP.Project.Database;

namespace IP.Project.Features.Samba.Tests
{
    public class RepositoryMocks
    {
        public static ApplicationDBContext GetDbContextMock()
        {
            var sambaAccounts = new List<SambaAccount>
            {
                new SambaAccount { Id = Guid.Parse("7d52a70d-091c-4d53-8f87-16c3a3b1d2c4"), Description = "description 1", IPv4Address = "2342342" },
            };

            var mockContext = new Mock<ApplicationDBContext>(new DbContextOptions<ApplicationDBContext>());
            
            mockContext.Setup(db => db.SambaAccounts).Returns(DbSetMockHelper.GetDbSetMock(sambaAccounts));

            return mockContext.Object;
        }
    }

    public static class DbSetMockHelper
    {
        public static DbSet<T> GetDbSetMock<T>(List<T> data) where T : class
        {
            var queryableData = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(queryableData.GetEnumerator()));

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(queryableData.Provider));

            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());

            return mockSet.Object;
        }
    }

}