using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Vpn;
using IP.Project.Tests.Base;
using NSubstitute;

namespace IP.Project.Tests.Features.Vpn;

public class DeleteVpnTests : BaseTest<VpnAccount>
{
    [Fact]
    public async Task DeleteVpnHandler_ValidData_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.Parse("9066e069-3097-4554-aaef-44c3382c9c28");
        var acc = new List<VpnAccount>
        {
            new() { Id = id, Description = "VPN Account 1", IPv4Address = "192.168.1.1" },
            new() { Id = Guid.Parse("9c4497f4-3c79-4bd0-bef1-e47b1e2cbcd9"), Description = "VPN Account 2", IPv4Address = "192.168.1.2" }
        };

        var mock = Setup(acc);

        var sut = new DeleteVpn.Handler(mock);
        var deleteCommand = new DeleteVpn.Command(id);

        // Act
        var vpnAccount = await sut.Handle(deleteCommand, default);

        // Assert
        vpnAccount.IsSuccess.Should().BeTrue();
        mock.Vpns.Received(1).Remove(Arg.Any<VpnAccount>());
        await mock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteVpnHandler_InvalidIpAddress_ReturnsFailure()
    {
        // Arrange
        var acc = new List<VpnAccount>
        {
            new() { Id = Guid.Parse("9066e069-3097-4554-aaef-44c3382c9c28"), Description = "VPN Account 1", IPv4Address = "192.168.1.1"}
        };
        var badId = Guid.Parse("a1491b26-9390-479c-b6c4-0ddddd8f0152");

        var mock = Setup(acc);

        var sut = new DeleteVpn.Handler(mock);
        var deleteCommand = new DeleteVpn.Command(badId);

        // Act
        var vpnAccount = await sut.Handle(deleteCommand, default);

        // Assert
        vpnAccount.IsFailure.Should().BeTrue();
        mock.Vpns.Received(0).Remove(Arg.Any<VpnAccount>());
        await mock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
