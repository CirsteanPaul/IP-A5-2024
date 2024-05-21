using Dapper;
using FluentAssertions;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Features.Vpn;
using IP.Project.Tests.Base;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using Moq;
using Moq.Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Configuration;
using Xunit;

namespace IP.Project.Tests.Features.Vpn
{
    public class GetVpnTests
    {
        private readonly Mock<ISqlConnectionFactory> mockSqlConnectionFactory;
        private readonly Mock<IDbConnection> mockDbConnection;
        private readonly GetVpn.Handler handler;
        
        public GetVpnTests()
        {
            mockSqlConnectionFactory = new Mock<ISqlConnectionFactory>();
            mockDbConnection = new Mock<IDbConnection>();
            mockSqlConnectionFactory.Setup(f => f.CreateConnection()).Returns(mockDbConnection.Object);
            handler = new GetVpn.Handler(mockSqlConnectionFactory.Object);
        }
        
        
        [Fact]
        public async Task GetVpnHandler_ValidId_ReturnsSuccess()
        {
            // Arrange
            var vpnId = Guid.Parse("4c727215-0522-4384-8481-4a2d1e094fb7");
            var vpnAccount = new VpnAccount { Id = vpnId, Description = "Test VPN", IPv4Address = "10.0.0.1" };
            
            //Config Dapper
            mockDbConnection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<VpnAccount>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                null,
                null,
                null)).ReturnsAsync(vpnAccount);
            
            var query = new GetVpn.Query { Id = vpnId };

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(vpnId);
            result.Value.Description.Should().Be("Test VPN");
            result.Value.IPv4Address.Should().Be("10.0.0.1");
        }
        [Fact]
        public async Task GetSambaHandler_InvalidId_ReturnsFailure()
        {
            // Arrange
            var invalidId = Guid.Parse("b0af913e-4b78-408d-98b3-8103fb3b1870");

            // Configurarea mock-ului pentru Dapper
            mockDbConnection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<VpnAccount>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                null,
                null,
                null)).ReturnsAsync((VpnAccount)null);

            var query = new GetVpn.Query { Id = invalidId };

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("GetVpn.Null");
            result.Error.Message.Should().Be("Vpn not found");
        }
    }
}


