/*
using Dapper;
using FluentAssertions;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Features.Vpn;
using IP.Project.Tests.Base;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IP.Project.Tests.Features.Vpn
{
    public class GetVpnTests : BaseTest<VpnAccount>
    {
        [Fact]
        public async Task GetVpnHandler_ValidId_ReturnsSuccess()
        {
            // Arrange
            var vpnId = Guid.Parse("4c727215-0522-4384-8481-4a2d1e094fb7");
            var vpnAccount = new VpnAccount { Id = vpnId, Description = "Test VPN", IPv4Address = "10.0.0.1" };

            // Mock pentru DbConnection
            var mockDbConnection = new Mock<DbConnection>();
            var dapperResult = new List<VpnAccount> { vpnAccount };
            mockDbConnection.Setup(d => d.QueryAsync<VpnAccount>(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
                            .ReturnsAsync(dapperResult);

            // Mock pentru ApplicationDBContext
            var mockDbContext = new Mock<ApplicationDBContext>(MockBehavior.Strict, null);
            mockDbContext.Setup(c => c.Database.GetDbConnection()).Returns(mockDbConnection.Object);

            var sut = new GetVpn.Handler(mockDbContext.Object);
            var query = new GetVpn.Query { Id = vpnId };

            // Act
            var result = await sut.Handle(query, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(vpnId);
            result.Value.Description.Should().Be("Test VPN");
            result.Value.IPv4Address.Should().Be("10.0.0.1");
        }
    }
}
*/
