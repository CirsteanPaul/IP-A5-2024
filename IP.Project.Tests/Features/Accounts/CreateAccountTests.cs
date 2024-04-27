using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Accounts;
using IP.Project.Tests.Base;
using NSubstitute;

namespace IP.Project.Tests.Features.Accounts;

public class CreateAccountTests : BaseTest<Account>
{
    [Fact]
    public async Task CreateAccountHandler_ValidData_ReturnsSuccess()
    {
        // Arrange
        var mock = Setup(new List<Account>());

        var sut = new CreateAccount.Handler(mock, new CreateAccount.Validator());
        var createCommand = new CreateAccount.Command()
        {
            Username = "test",
            Password = "test",
            Email = "test@test.com",
            Matricol = "123456789ABC123456"
        };

        // Act
        var account = await sut.Handle(createCommand, default);

        // Assert
        account.IsSuccess.Should().BeTrue();
        mock.Received(1).Accounts.Add(Arg.Any<Account>());
        await mock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAccountHandler_InvalidEmail_ReturnsFailure()
    {
        // Arrange
        var mock = Setup(new List<Account>());

        var sut = new CreateAccount.Handler(mock, new CreateAccount.Validator());
        var createCommand = new CreateAccount.Command()
        {
            Username = "test",
            Password = "test",
            Email = "test",
            Matricol = "123456789ABC123456"
        };

        // Act
        var account = await sut.Handle(createCommand, default);

        // Assert
        account.IsFailure.Should().BeTrue();
        mock.Received(0).Accounts.Add(Arg.Any<Account>());
        await mock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAccountHandler_InvalidMatricol_ReturnsFailure()
    {
        // Arrange
        var mock = Setup(new List<Account>());

        var sut = new CreateAccount.Handler(mock, new CreateAccount.Validator());
        var createCommand = new CreateAccount.Command()
        {
            Username = "test",
            Password = "test",
            Email = "test@test.com",
            Matricol = "123456789123456"
        };

        // Act
        var account = await sut.Handle(createCommand, default);

        // Assert
        account.IsFailure.Should().BeTrue();
        mock.Received(0).Accounts.Add(Arg.Any<Account>());
        await mock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAccountHandler_EmptyUsername_ReturnsFailure()
    {
        // Arrange
        var mock = Setup(new List<Account>());

        var sut = new CreateAccount.Handler(mock, new CreateAccount.Validator());
        var createCommand = new CreateAccount.Command()
        {
            Username = "",
            Password = "test",
            Email = "test@test.com",
            Matricol = "123456789ABC123456"
        };

        // Act
        var account = await sut.Handle(createCommand, default);

        // Assert
        account.IsFailure.Should().BeTrue();
        mock.Received(0).Accounts.Add(Arg.Any<Account>());
        await mock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
