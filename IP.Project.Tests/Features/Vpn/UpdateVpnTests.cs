using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using IP.Project.Contracts.Vpn;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Features.Vpn;
using IP.Project.Tests.Base;
using NSubstitute;


namespace IP.Project.Tests.Features.Vpn
{
    public class UpdateVpnTests : BaseTest<VpnAccount>
    {
        [Fact]
        public async Task Handle_GivenValidId_UpdatesVpn()
        {

            var vpnId = Guid.NewGuid();
            var oldIpAddress = "192.168.0.1";
            var oldDescription = "Initial Description";
            var newIpAddress = "192.168.0.2";
            var newDescription = "Updated Description";
            var vpn = new VpnAccount
            {
                Id = vpnId,
                IPv4Address = oldIpAddress,
                Description = oldDescription
            };

            var mockSet = Substitute.For<DbSet<VpnAccount>>();
            var mockContext = Substitute.For<ApplicationDBContext>(new DbContextOptions<ApplicationDBContext>());
            mockSet.FindAsync(vpnId, default(CancellationToken)).Returns(vpn);
            mockContext.Vpns.Returns(mockSet);

            var handler = new UpdateVpnInstance.Handler(mockContext);
            var command = new UpdateVpnInstance.Command(vpnId, new UpdateVpnRequest
            {
                NewIpAddress = newIpAddress,
                NewDescription = newDescription,
            });
            
            var result = await handler.Handle(command, CancellationToken.None);
            
            result.IsSuccess.Should().BeTrue();
            vpn.IPv4Address.Should().Be(newIpAddress);
            vpn.Description.Should().Be(newDescription);
            await mockContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_GivenInvalidId_ReturnsFailure()
        {
            // Arrange
            var id = Guid.Parse("4c727215-0522-4384-8481-4a2d1e094fb7");
            var vpnAccount = new VpnAccount { Id = id, Description = "Vpn Account 1", IPv4Address = "192.168.1.1" };
            var dbContextMock = Setup(new List<VpnAccount> { vpnAccount });

            var request = new UpdateVpnInstance.Command(id, new UpdateVpnRequest
            {
                NewIpAddress = "192.168.1", // Invalid IP address format
                NewDescription = "Modified Vpn Account 1" // Valid description
            });
            var handler = new UpdateVpnInstance.Handler(dbContextMock);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            await dbContextMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
            vpnAccount.IPv4Address.Should().NotBe("192.168.1"); // IP address should not be updated
            vpnAccount.Description.Should().NotBe("Modified Vpn Account 1"); // Description should not be updated
        }
    }
}
