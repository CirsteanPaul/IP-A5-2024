using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Features.Vpn;
using NSubstitute;


namespace IP.Project.Tests.Features.Vpn
{
    public class UpdateVpnTests
    {
        [Fact]
        public async Task Handle_GivenValidId_UpdatesVpn()
        {

            var vpnId = Guid.NewGuid();
            var oldIpAddress = "192.168.0.1";
            var oldDescription = "Initial Description";
            var newIpAddress = "192.168.0.2";
            var newDescription = "Updated Description";
            var vpn = new Entities.VpnAccount
            {
                Id = vpnId,
                IPv4Address = oldIpAddress,
                Description = oldDescription
            };

            var mockSet = Substitute.For<DbSet<Entities.VpnAccount>>();
            var mockContext = Substitute.For<ApplicationDBContext>(new DbContextOptions<ApplicationDBContext>());
            mockSet.FindAsync(vpnId, default(CancellationToken)).Returns(vpn);
            mockContext.Vpns.Returns(mockSet);

            var handler = new UpdateVpnInstance.Handler(mockContext);
            var command = new UpdateVpnInstance.Command(vpnId, newIpAddress, newDescription);
            
            var result = await handler.Handle(command, CancellationToken.None);
            
            result.IsSuccess.Should().BeTrue();
            vpn.IPv4Address.Should().Be(newIpAddress);
            vpn.Description.Should().Be(newDescription);
            await mockContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_GivenInvalidId_ReturnsFailure()
        {
            var vpnId = Guid.NewGuid();
            var mockContext = Substitute.For<ApplicationDBContext>(new DbContextOptions<ApplicationDBContext>());
            var mockSet = Substitute.For<DbSet<VpnAccount>>();

            mockSet.FindAsync(vpnId, default(CancellationToken)).Returns((VpnAccount)null);
            mockContext.Vpns.Returns(mockSet);

            var handler = new UpdateVpnInstance.Handler(mockContext);
            var command = new UpdateVpnInstance.Command(vpnId, "192.168.0.2", "Updated Description");
            
            var result = await handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("UpdateVpn.Null");
            result.Error.Message.Should().Contain($"Vpn instance with ID {vpnId} not found");
        }
    }
}
