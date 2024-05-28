using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Accounts;
using IP.Project.Tests.Base;
using IP.Project.Contracts;
using IP.Project.Contracts.Account;
using IP.Project.Contracts.Samba;
using NSubstitute;

namespace IP.Project.Tests.Features.Accounts;

public class UpdateAccountTests : BaseTest<Account>
{
    [Fact]
    public async Task UpdateAccountHandler_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.Parse("f1302aae-193b-4ae0-8d1d-b8a3f239fff1");
        var acc = new List<Account>
        {
            new() { Id = id, Username = "TestUser1", Password = "TestPassword1", Email = "test@test.com", Matricol = "123456789ABC123456", CreatedOnUtc=DateTime.UtcNow, LastUpdatedOnUtc = DateTime.UtcNow },
        };
        var dbContextMock = Setup(acc);

        var request = new UpdateAccountInstance.Command(id, new UpdateAccountRequest
        {
            NewUsername = "TestUser2", 
            NewPassword = "TestPassword2", 
            NewEmail = "test2@test.com"
        });
        var handler = new UpdateAccountInstance.Handler(dbContextMock);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await dbContextMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        acc[0].Username.Should().Be("TestUser2");
        acc[0].Password.Should().Be("TestPassword2");
        acc[0].Email.Should().Be("test2@test.com");
    }

    [Fact]
    public async Task UpdateAccountHandler_InvalidEmail_ReturnsFailure()
    {
        // Arrange
        var id = Guid.Parse("f1302aae-193b-4ae0-8d1d-b8a3f239fff1");
        var acc = new List<Account>
        {
            new() { Id = id, Username = "TestUser1", Password = "TestPassword1", Email = "test@test.com", Matricol = "123456789ABC123456", CreatedOnUtc=DateTime.UtcNow, LastUpdatedOnUtc = DateTime.UtcNow },
        };
        var dbContextMock = Setup(acc);

        var request = new UpdateAccountInstance.Command(id, new UpdateAccountRequest
        {
            NewUsername = "TestUser2",
            NewPassword = "TestPassword2",
            NewEmail = "test2"
        });
        var handler = new UpdateAccountInstance.Handler(dbContextMock);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await dbContextMock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
        acc[0].Username.Should().Be("TestUser1");
        acc[0].Password.Should().Be("TestPassword1");
        acc[0].Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task UpdateAccountHandler_NullUsername_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.Parse("f1302aae-193b-4ae0-8d1d-b8a3f239fff1");
        var acc = new List<Account>
        {
            new() { Id = id, Username = "TestUser1", Password = "TestPassword1", Email = "test@test.com", Matricol = "123456789ABC123456", CreatedOnUtc=DateTime.UtcNow, LastUpdatedOnUtc = DateTime.UtcNow },
        };
        var dbContextMock = Setup(acc);

        var request = new UpdateAccountInstance.Command(id, new UpdateAccountRequest
        {
            NewPassword = "TestPassword2",
            NewEmail = "test2@test.com"
        });
        var handler = new UpdateAccountInstance.Handler(dbContextMock);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await dbContextMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        acc[0].Username.Should().Be("TestUser1");
        acc[0].Password.Should().Be("TestPassword2");
        acc[0].Email.Should().Be("test2@test.com");
    }

    [Fact]
    public async Task UpdateAccountHandler_NullPassword_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.Parse("f1302aae-193b-4ae0-8d1d-b8a3f239fff1");
        var acc = new List<Account>
        {
            new() { Id = id, Username = "TestUser1", Password = "TestPassword1", Email = "test@test.com", Matricol = "123456789ABC123456", CreatedOnUtc=DateTime.UtcNow, LastUpdatedOnUtc = DateTime.UtcNow },
        };
        var dbContextMock = Setup(acc);

        var request = new UpdateAccountInstance.Command(id, new UpdateAccountRequest
        {
            NewUsername = "TestUser2",
            NewEmail = "test2@test.com"
        });
        var handler = new UpdateAccountInstance.Handler(dbContextMock);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await dbContextMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        acc[0].Username.Should().Be("TestUser2");
        acc[0].Password.Should().Be("TestPassword1");
        acc[0].Email.Should().Be("test2@test.com");
    }

    [Fact]
    public async Task UpdateAccountHandler_NullEmail_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.Parse("f1302aae-193b-4ae0-8d1d-b8a3f239fff1");
        var acc = new List<Account>
        {
            new() { Id = id, Username = "TestUser1", Password = "TestPassword1", Email = "test@test.com", Matricol = "123456789ABC123456", CreatedOnUtc=DateTime.UtcNow, LastUpdatedOnUtc = DateTime.UtcNow },
        };
        var dbContextMock = Setup(acc);

        var request = new UpdateAccountInstance.Command(id, new UpdateAccountRequest
        {
            NewUsername = "TestUser2",
            NewPassword = "TestPassword2"
        });
        var handler = new UpdateAccountInstance.Handler(dbContextMock);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await dbContextMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        acc[0].Username.Should().Be("TestUser2");
        acc[0].Password.Should().Be("TestPassword2");
        acc[0].Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task UpdateAccountHandler_NullAll_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.Parse("f1302aae-193b-4ae0-8d1d-b8a3f239fff1");
        var acc = new List<Account>
        {
            new() { Id = id, Username = "TestUser1", Password = "TestPassword1", Email = "test@test.com", Matricol = "123456789ABC123456", CreatedOnUtc=DateTime.UtcNow, LastUpdatedOnUtc = DateTime.UtcNow },
        };
        var dbContextMock = Setup(acc);

        var request = new UpdateAccountInstance.Command(id, new UpdateAccountRequest
        {
        });
        var handler = new UpdateAccountInstance.Handler(dbContextMock);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await dbContextMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        acc[0].Username.Should().Be("TestUser1");
        acc[0].Password.Should().Be("TestPassword1");
        acc[0].Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task UpdateAccountHandler_VerifyTimestamps()
    {
        // Arrange
        var id = Guid.Parse("f1302aae-193b-4ae0-8d1d-b8a3f239fff1");
        var createdOn = DateTime.UtcNow;
        var updatedOn = DateTime.UtcNow;
        var acc = new List<Account>
        {
            new() { Id = id, Username = "TestUser1", Password = "TestPassword1", Email = "test@test.com", Matricol = "123456789ABC123456", CreatedOnUtc=createdOn, LastUpdatedOnUtc = updatedOn },
        };
        var dbContextMock = Setup(acc);

        var request = new UpdateAccountInstance.Command(id, new UpdateAccountRequest
        {
        });
        var handler = new UpdateAccountInstance.Handler(dbContextMock);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await dbContextMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        acc[0].Username.Should().Be("TestUser1");
        acc[0].Password.Should().Be("TestPassword1");
        acc[0].Email.Should().Be("test@test.com");
        acc[0].CreatedOnUtc.Should().Be(createdOn);
        acc[0].LastUpdatedOnUtc.Should().BeAfter(updatedOn);
    }

}