using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Accounts;
using IP.Project.Tests.Base;
using Moq;


namespace IP.Project.Tests.Features.Accounts;

public class DeleteAccountTests : BaseTest<Account>
{
    [Fact]
    public async Task DeleteAccountHandler_ExistingId_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var acc = new List<Account>
        {
            new() { Id = id, Username = "TestUser1", Password = "TestPassword1", Email = "test@test.com", Matricol = "123456789ABC123456", CreatedOnUtc=DateTime.UtcNow, LastUpdatedOnUtc = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Username = "TestUser1", Password = "TestPassword1", Email = "test@test.com", Matricol = "123456789ABC123456", CreatedOnUtc=DateTime.UtcNow, LastUpdatedOnUtc = DateTime.UtcNow },
        };

        var mock = Setup(acc);

        var sut = new DeleteAccount.Handler(mock.Object);
        var deleteCommand = new DeleteAccount.Command(id);

        // Act
        var account = await sut.Handle(deleteCommand, default);

        // Assert
        account.IsSuccess.Should().BeTrue();
        mock.Verify(x => x.Accounts.Remove(It.IsAny<Account>()), Times.Once);
        mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
