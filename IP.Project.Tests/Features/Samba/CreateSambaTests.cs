using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Samba;
using IP.Project.Tests.Base;
using NSubstitute;

namespace IP.Project.Tests.Features.Samba;

public class CreateSambaTests : BaseTest<SambaAccount>
{
    [Fact]
    public async Task CreateSambaHandler_InvalidIpAddress_ReturnsFailure()
    {
        // Arrange
        var acc = new List<SambaAccount>();
        var mock = Setup(acc);

        var sut = new CreateSamba.Handler(mock, new CreateSamba.Validator());
        var createCommand = new CreateSamba.Command()
        {
            Description = "Samba Account 1",
            IPv4Address = "182.158" 
        };
        
        // Act
        var sambaAccount = await sut.Handle(createCommand, default);

        // Assert
        sambaAccount.IsFailure.Should().BeTrue();
        mock.Received(0).SambaAccounts.Add(Arg.Any<SambaAccount>());
        await mock.Received(0).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    
    public async Task CreateSambaHandler_ValidIpAddressAndNullDescription_ReturnsSucces()
    {
        // Arrange
        var acc = new List<SambaAccount>();
        var mock = Setup(acc);

        var sut = new CreateSamba.Handler(mock, new CreateSamba.Validator());
        var createCommand = new CreateSamba.Command()
        {
            Description = null,
            IPv4Address = "192.168.1.1" 
        };
        
        // Act
        var sambaAccount = await sut.Handle(createCommand, default);

        // Assert
        sambaAccount.IsSuccess.Should().BeTrue();
        mock.Received(1).SambaAccounts.Add(Arg.Any<SambaAccount>());
        await mock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    
    public async Task CreateSambaHandler_ValidData_ReturnsSucces()
    {
        // Arrange
        var acc = new List<SambaAccount>();
        var mock = Setup(acc);

        var sut = new CreateSamba.Handler(mock, new CreateSamba.Validator());
        var createCommand = new CreateSamba.Command()
        {
            Description = "Samba account 2",
            IPv4Address = "192.168.1.1" 
        };
        
        // Act
        var sambaAccount = await sut.Handle(createCommand, default);

        // Assert
        sambaAccount.IsSuccess.Should().BeTrue();
        mock.Received(1).SambaAccounts.Add(Arg.Any<SambaAccount>());
        await mock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}