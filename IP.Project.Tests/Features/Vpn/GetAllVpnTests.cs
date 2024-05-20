using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Vpn;
using IP.Project.Tests.Base;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Xunit;

namespace IP.Project.Tests.Features.Vpn
{
    public class GetAllVpnTests : BaseTest<VpnAccount>
    {
        [Fact]
        public async Task GetAllVpnHandler_ReturnsListOfVpnAccounts()
        {
            // Arrange
            var vpnAccounts = new List<VpnAccount>
            {
                new VpnAccount { Id = Guid.NewGuid(), Description = "VPN 1", IPv4Address = "192.168.1.1" },
                new VpnAccount { Id = Guid.NewGuid(), Description = "VPN 2", IPv4Address = "192.168.1.2" }
            };

            var mockDbConnection = SetupDapper.SetupDapperForVpn(vpnAccounts);
            var sut = new GetAllVpns.Handler(mockDbConnection.Object);
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
            var mockDbConnection = SetupDapper.SetupDapperForVpn(new List<VpnAccount>());
            var sut = new GetAllVpns.Handler(mockDbConnection.Object);
            var query = new GetAllVpns.Query();

            // Act
            var result = await sut.Handle(query, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }
    }
}
