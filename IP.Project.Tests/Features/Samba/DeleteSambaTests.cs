using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Samba;
using IP.Project.Tests.Base;
using Moq;

namespace IP.Project.Tests.Features.Samba;


public class DeleteSambaTests : BaseTest<SambaAccount>
{
    [Fact]
    public async Task DeleteSambaHandler_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var acc = new List<SambaAccount>
        {
            new() { Id = id, Description = "Samba Account 1", IPv4Address = "192.168.1.1" },
            new() { Id = Guid.NewGuid(), Description = "Samba Account 2", IPv4Address = "192.168.1.2" }
        };

        var mock = Setup(acc);

        var sut = new DeleteSamba.Handler(mock.Object);
        var deleteCommand = new DeleteSamba.Command(id);
        
        // Act
        var sambaAccount = await sut.Handle(deleteCommand, default);

        // Assert
        sambaAccount.IsSuccess.Should().BeTrue();
        mock.Verify(x => x.SambaAccounts.Remove(It.IsAny<SambaAccount>()), Times.Once);
        mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}