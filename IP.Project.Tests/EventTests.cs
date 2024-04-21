using Xunit;
using IP.Project.Features.Samba;
using IP.Project.Entities;
using IP.Project.Database;
using Moq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace IP.Project.Features.Samba.Tests
{
    public class GetSambaTests
    {
        // [Fact]
        // public async Task GetSamba_ReturnsCorrectSambaAccount()
        // {
        //     // Arrange
        //     var sambaAccounts = new List<SambaAccount>
        //     {
        //         new SambaAccount { Id = Guid.NewGuid(), Description = "Samba Account 1", IPv4Address = "192.168.1.1" },
        //         new SambaAccount { Id = Guid.NewGuid(), Description = "Samba Account 2", IPv4Address = "192.168.1.2" }
        //     };
        //     
        //     var mockDbContext = new Mock<ApplicationDBContext>(new DbContextOptions<ApplicationDBContext>());
        //     mockDbContext.Setup(db => db.SambaAccounts).Returns(DbSetMockHelper.GetDbSetMock(sambaAccounts));
        //     
        //     var applicationDbContext = mockDbContext.Object;
        //     var sambaId = sambaAccounts.First().Id;
        //     var query = new GetSamba.Query { Id = sambaId };
        //     
        //     var handler = new GetSamba.Handler(applicationDbContext);
        //
        //     // Act
        //     var result = await handler.Handle(query, CancellationToken.None);
        //
        //     // Assert
        //     Assert.True(result.IsSuccess);
        // }

        [Fact]
        public async Task Something()
        {
            // Arrange
            var sambaAccounts = new List<SambaAccount>
            {
                new SambaAccount { Id = Guid.NewGuid(), Description = "Samba Account 1", IPv4Address = "192.168.1.1" },
                new SambaAccount { Id = Guid.NewGuid(), Description = "Samba Account 2", IPv4Address = "192.168.1.2" }
            };

            var mockDbContext = new Mock<ApplicationDBContext>(new DbContextOptions<ApplicationDBContext>());
            mockDbContext.Setup(db => db.SambaAccounts).Returns(DbSetMockHelper.GetDbSetMock(sambaAccounts));

            var applicationDbContext = mockDbContext.Object;

            // Act
            var result = await applicationDbContext.SambaAccounts.FirstOrDefaultAsync();
            
            // Assert
            // Assert.NotNull(result);
            // Assert.Equal("Samba Account 1", result.Description); 
        }

    }
}
