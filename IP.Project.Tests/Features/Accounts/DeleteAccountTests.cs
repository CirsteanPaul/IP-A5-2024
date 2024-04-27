using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Accounts;
using IP.Project.Tests.Base;
using NSubstitute;

namespace IP.Project.Tests.Features.Accounts;

public class DeleteAccountTests : BaseTest<Account>
{
    [Fact]
    public async Task DeleteAccountHandler_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.Parse("f1302aae-193b-4ae0-8d1d-b8a3f239fff1");
        var acc = new List<Account>
        {
            new() { Id = id, Username = "TestUser1", Password = "TestPassword1", Email = "test@test.com", Matricol = "123456789ABC123456", CreatedOnUtc=DateTime.UtcNow, LastUpdatedOnUtc = DateTime.UtcNow },
        };

        var mock = Setup(acc);

        var sut = new DeleteAccount.Handler(mock);
        var deleteCommand = new DeleteAccount.Command(id);

        // Act
        var account = await sut.Handle(deleteCommand, default);

        // Assert
        account.IsSuccess.Should().BeTrue();
        mock.Accounts.Received(1).Remove(Arg.Any<Account>());
        await mock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAccountHandler_NonExistingId_ReturnsFailure()
    {
        // Arrange
        var badId = Guid.Parse("a1491b26-9390-479c-b6c4-0d21ce8f0152");

        var mock = Setup(new List<Account>());

        var sut = new DeleteAccount.Handler(mock);
        var deleteCommand = new DeleteAccount.Command(badId);

        // Act
        var account = await sut.Handle(deleteCommand, default);

        // Assert
        account.IsFailure.Should().BeTrue();
        mock.Accounts.Received(0).Remove(Arg.Any<Account>());
        await mock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
