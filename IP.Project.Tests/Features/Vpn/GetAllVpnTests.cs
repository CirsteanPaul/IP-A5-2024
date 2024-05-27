﻿using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Vpn;
using IP.Project.Tests.Base;
using System.Data;
using NSubstitute;
using NSubstitute.DbConnection;

namespace IP.Project.Tests.Features.Vpn
{
    public class GetAllVpnTests : BaseTest<VpnAccount>
    {
        [Fact]
        public async Task GetAllVpnHandler_ReturnsListOfVpnAccounts()
        {
            // Arrange
            IEnumerable<VpnAccount> vpnAccounts = new List<VpnAccount>
            {
                new VpnAccount { Id = Guid.NewGuid(), Description = "VPN 1", IPv4Address = "192.168.1.1" },
                new VpnAccount { Id = Guid.NewGuid(), Description = "VPN 2", IPv4Address = "192.168.1.2" }
            };

            var mock = SetupDapper(() =>
            {
                var sqlConnection = Substitute.For<IDbConnection>().SetupCommands();
                sqlConnection
                    .SetupQuery("SELECT * FROM Vpns")
                    .Returns(vpnAccounts);

                return sqlConnection;
            });
            var sut = new GetAllVpns.Handler(mock);
            var query = new GetAllVpns.Query();

            // Act
            var result = await sut.Handle(query, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();
            result.Value.Should().HaveCount(2);
            result.Value.Should().ContainEquivalentOf(new { Description = "VPN 1", IPv4Address = "192.168.1.1" });
            result.Value.Should().ContainEquivalentOf(new { Description = "VPN 2", IPv4Address = "192.168.1.2" });
        }

        [Fact]
        public async Task GetAllVpnHandler_NoVpnAccounts_ReturnsEmptyList()
        {
            // Arrange
            var mock = SetupDapper(() =>
            {
                var sqlConnection = Substitute.For<IDbConnection>().SetupCommands();
                sqlConnection
                    .SetupQuery("SELECT * FROM Vpns")
                    .Returns(new List<VpnAccount>());

                return sqlConnection;
            });
            var sut = new GetAllVpns.Handler(mock);
            var query = new GetAllVpns.Query();

            // Act
            var result = await sut.Handle(query, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Should().BeEmpty();
        }
    }
}