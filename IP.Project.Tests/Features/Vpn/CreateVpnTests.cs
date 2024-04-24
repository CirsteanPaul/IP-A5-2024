using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Vpn;
using IP.Project.Tests.Base;
using NSubstitute;

namespace IP.Project.Tests.Features.Vpn;

public class CreateVpnTests : BaseTest<VpnAccount>
{
    [Fact]
    public async Task CreateVpnHandler_ValidData_ReturnsSuccess()
    {
        // Arrange
        var acc = new List<VpnAccount>();
        var mock = Setup(acc);

        var sut = new CreateVpn.Handler(mock, new CreateVpn.Validator());
        var createCommand = new CreateVpn.Command()
        {
            Description = "VPN Account 1",
            IPv4Address = "192.168.1.1"
        };

        // Act
        var vpnAccount = await sut.Handle(createCommand, default);

        // Assert
        vpnAccount.IsSuccess.Should().BeTrue();
        mock.Vpns.Received(1).Add(Arg.Any<VpnAccount>());
        await mock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateVpnHandler_InvalidIpAddress_ReturnsFailure() 
    {
        // Arrange
        var acc = new List<VpnAccount>();
        var mock = Setup(acc);

        var sut = new CreateVpn.Handler(mock, new CreateVpn.Validator());
        var createCommand = new CreateVpn.Command()
        {
            Description = "VPN Account 1",
            IPv4Address = "192.168" 
        };

        // Act
        var vpnAccount = await sut.Handle(createCommand, default);

        // Assert
        vpnAccount.IsFailure.Should().BeTrue();
        mock.Vpns.Received(0).Add(Arg.Any<VpnAccount>());
        await mock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
