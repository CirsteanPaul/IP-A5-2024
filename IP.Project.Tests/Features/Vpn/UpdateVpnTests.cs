using Moq;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using IP.Project.Database;
using IP.Project.Features.Vpn;


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
            var vpn = new Entities.Vpn
            {
                Id = vpnId,
                IPv4Address = oldIpAddress,
                Description = oldDescription
            };

            var mockSet = new Mock<DbSet<Entities.Vpn>>();
            var mockContext = new Mock<ApplicationDBContext>(new DbContextOptions<ApplicationDBContext>());

            mockSet.Setup(m => m.FindAsync(vpnId, default(CancellationToken)))
                   .ReturnsAsync(vpn);
            mockContext.Setup(c => c.Vpns)
                       .Returns(mockSet.Object);

            var handler = new UpdateVpnInstance.Handler(mockContext.Object);
            var command = new UpdateVpnInstance.Command(vpnId, newIpAddress, newDescription);


            var result = await handler.Handle(command, CancellationToken.None);


            result.IsSuccess.Should().BeTrue();
            vpn.IPv4Address.Should().Be(newIpAddress);
            vpn.Description.Should().Be(newDescription);
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_GivenInvalidId_ReturnsFailure()
        {

            var vpnId = Guid.NewGuid();
            var mockSet = new Mock<DbSet<Entities.Vpn>>();
            var mockContext = new Mock<ApplicationDBContext>(new DbContextOptions<ApplicationDBContext>());

            mockSet.Setup(m => m.FindAsync(vpnId, default(CancellationToken)))
                   .ReturnsAsync((Entities.Vpn)null);
            mockContext.Setup(c => c.Vpns)
                       .Returns(mockSet.Object);

            var handler = new UpdateVpnInstance.Handler(mockContext.Object);
            var command = new UpdateVpnInstance.Command(vpnId, "192.168.0.2", "Updated Description");


            var result = await handler.Handle(command, CancellationToken.None);


            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("UpdateVpn.Null");
            result.Error.Message.Should().Contain($"Vpn instance with ID {vpnId} not found");
        }
    }
}
