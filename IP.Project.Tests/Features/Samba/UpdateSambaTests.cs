using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Samba;
using IP.Project.Tests.Base;
using NSubstitute;

namespace IP.Project.Tests.Features.Samba
{
    public class UpdateSambaInstanceTests : BaseTest<SambaAccount>
    {
        [Fact]
        public async Task Handle_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();
            var sambaAccount = new SambaAccount { Id = id, Description = "Samba Account 1", IPv4Address = "192.168.1.1" };
            var dbContextMock = Setup(new List<SambaAccount> { sambaAccount });

            var request = new UpdateSambaInstance.Command(id, new UpdateSambaRequest
            {
                NewIpAddress = "192.168.1.100", // Valid IP address
                NewDescription = "Modified Samba Account 1" // Valid description
            });
            var handler = new UpdateSambaInstance.Handler(dbContextMock);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            await dbContextMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            sambaAccount.IPv4Address.Should().Be("192.168.1.100"); // IP address should be updated
            sambaAccount.Description.Should().Be("Modified Samba Account 1"); // Description should remain the same
        }

        [Fact]
        public async Task Handle_NullDescription_ReturnsSuccess()
        {
            // Arrange
            var id = Guid.NewGuid();
            var sambaAccount = new SambaAccount { Id = id, Description = "Samba Account 1", IPv4Address = "192.168.1.1" };
            var dbContextMock = Setup(new List<SambaAccount> { sambaAccount });

            var request = new UpdateSambaInstance.Command(id, new UpdateSambaRequest
            {
                NewIpAddress = "192.168.1.2", // Valid IP address
                NewDescription = null // Description left as null
            });
            var handler = new UpdateSambaInstance.Handler(dbContextMock);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            await dbContextMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            sambaAccount.IPv4Address.Should().Be("192.168.1.2"); // IP address should be updated
            sambaAccount.Description.Should().Be("Samba Account 1"); // Description should remain the same
        }

        [Fact]
        public async Task Handle_InvalidIpAddress_ReturnsFailure()
        {
            // Arrange
            var id = Guid.NewGuid();
            var sambaAccount = new SambaAccount { Id = id, Description = "Samba Account 1", IPv4Address = "192.168.1.1" };
            var dbContextMock = Setup(new List<SambaAccount> { sambaAccount });

            var request = new UpdateSambaInstance.Command(id, new UpdateSambaRequest
            {
                NewIpAddress = "192.168.1", // Invalid IP address format
                NewDescription = "Modified Samba Account 1" // Valid description
            });
            var handler = new UpdateSambaInstance.Handler(dbContextMock);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            await dbContextMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
            sambaAccount.IPv4Address.Should().NotBe("192.168.1"); // IP address should not be updated
            sambaAccount.Description.Should().NotBe("Modified Samba Account 1"); // Description should not be updated
        }
    }
}
