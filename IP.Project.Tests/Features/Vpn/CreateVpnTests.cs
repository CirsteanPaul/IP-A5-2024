using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Vpn;
using IP.Project.Tests.Base;
using Moq;

namespace IP.Project.Tests.Features.Vpn;

public class CreateVpnTests : BaseTest<VpnAccount>
{
    [Fact]
    public async Task CreateVpnHandler_ValidData_ReturnsSuccess()
    {
        // Arrange
        var acc = new List<VpnAccount>();
        var mock = Setup(acc);

        var sut = new CreateVpn.Handler(mock.Object, new CreateVpn.Validator());
        var createCommand = new CreateVpn.Command()
        {
            Description = "VPN Account 1",
            IPv4Address = "192.168.1.1"
        };

        // Act
        var vpnAccount = await sut.Handle(createCommand, default);

        // Assert
        vpnAccount.IsSuccess.Should().BeTrue();
        mock.Verify(x => x.Vpns.Add(It.IsAny<VpnAccount>()), Times.Once);
        mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateVpnHandler_InvalidIpAddress_ReturnsFailure() 
    {
        // Arrange
        var acc = new List<VpnAccount>();
        var mock = Setup(acc);

        var sut = new CreateVpn.Handler(mock.Object, new CreateVpn.Validator());
        var createCommand = new CreateVpn.Command()
        {
            Description = "VPN Account 1",
            IPv4Address = "192.168" 
        };

        // Act
        var vpnAccount = await sut.Handle(createCommand, default);

        // Assert
        vpnAccount.IsFailure.Should().BeTrue();
        mock.Verify(x => x.Vpns.Add(It.IsAny<VpnAccount>()), Times.Never);
        mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
