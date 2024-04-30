using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Vpn;
using IP.Project.Contracts;
using IP.Project.Tests.Base;
using NSubstitute;


namespace IP.Project.Tests.Features.Vpn
{
    public class UpdateVpnTests : BaseTest<VpnAccount>
    {
        [Fact]
        public async Task Handle_GivenValidId_UpdatesVpn()
        {
            // Arrange
            var id = Guid.Parse("4c727215-0522-4384-8481-4a2d1e094fb7");
            var vpn = new VpnAccount { Id = id, Description = "Vpn 1", IPv4Address = "192.168.1.1" };
            var dbContextMock = Setup(new List<VpnAccount> { vpn });

            var request = new UpdateVpnInstance.Command(id, new UpdateVpnRequest
            {
                NewIpAddress = "192.168.1.100",
                NewDescription = "Modified Vpn 1"
            });
            var handler = new UpdateVpnInstance.Handler(dbContextMock);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            await dbContextMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            vpn.IPv4Address.Should().Be("192.168.1.100");
            vpn.Description.Should().Be("Modified Vpn 1");
        }

        [Fact]
        public async Task Handle_GivenInvalidId_ReturnsFailure()
        {
            // Arrange
            var id = Guid.Parse("4c727215-0522-4384-8481-4a2d1e094fb7");
            var acc = new List<VpnAccount>
            {
                new() { Id = Guid.Parse("9066e069-3097-4554-aaef-44c3382c9c28"), Description = "VPN Account 2", IPv4Address = "192.168.1.1"}
            };

            var mock = Setup(acc);

            var command = new UpdateVpnInstance.Command(id, new UpdateVpnRequest
            {
                NewIpAddress = "192.168.0.2",
                NewDescription = "Updated Description"
            });
            var handler = new UpdateVpnInstance.Handler(mock);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);


            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("UpdateVpn.Null");
            result.Error.Message.Should().Contain($"Vpn instance with ID {id} not found.");
        }
    }
}
