using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Vpn;
using IP.Project.Tests.Base;

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
            var mock = Setup(new List<VpnAccount> { vpnAccount });

            var sut = new GetVpn.Handler(mock);
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

        [Fact]
        public async Task GetVpnHandler_InvalidId_ReturnsFailure()
        {
            // Arrange
            var invalidId = Guid.Parse("b0af913e-4b78-408d-98b3-8103fb3b1870");
            var mock = Setup(new List<VpnAccount>());
            var sut = new GetVpn.Handler(mock);
            var query = new GetVpn.Query { Id = invalidId };

            // Act
            var result = await sut.Handle(query, default);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("GetVpn.Null");
            result.Error.Message.Should().Be("Vpn not found");
        }
    }
}