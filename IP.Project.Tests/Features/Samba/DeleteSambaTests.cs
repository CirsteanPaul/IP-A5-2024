using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Samba;
using IP.Project.Tests.Base;
using NSubstitute;

namespace IP.Project.Tests.Features.Samba;

public class DeleteSambaTests : BaseTest<SambaAccount>
{
    [Fact]
    public async Task DeleteSambaHandler_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.Parse("b6278728-fc58-4cc0-8444-a5699e29d70b");
        var acc = new List<SambaAccount>
        {
            new() { Id = id, Description = "Samba Account 1", IPv4Address = "192.168.1.1" },
            new() { Id = Guid.Parse("b6278728-fc58-4cc0-8444-a5699e29d70b"), Description = "Samba Account 2", IPv4Address = "192.168.1.2" }
        };

        var mock = Setup(acc);

        var sut = new DeleteSamba.Handler(mock);
        var deleteCommand = new DeleteSamba.Command(id);

        // Act
        var sambaAccount = await sut.Handle(deleteCommand, default);

        // Assert
        sambaAccount.IsSuccess.Should().BeTrue();
        mock.SambaAccounts.Received(1).Remove(Arg.Any<SambaAccount>());
        await mock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
