using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Samba;
using IP.Project.Tests.Base;
using Moq;

namespace IP.Project.Tests.Features.Samba;

public class CreateSambaTests : BaseTest<SambaAccount>
{
    [Fact]
    
    public async Task CreateSambaHandler_InvalidIpAddress_ReturnsFailure()
    {
        // Arrange
        var acc = new List<SambaAccount>();
        var mock = Setup(acc);

        var sut = new CreateSamba.Handler(mock.Object, new CreateSamba.Validator());
        var createCommand = new CreateSamba.Command()
        {
            Description = "Samba Account 1",
            IPv4Address = "182.158" 
        };
        
        // Act
        var sambaAccount = await sut.Handle(createCommand, default);

        // Assert
        sambaAccount.IsFailure.Should().BeTrue();
        mock.Verify(x => x.SambaAccounts.Add(It.IsAny<SambaAccount>()), Times.Never);
        mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    
    public async Task CreateSambaHandler_ValidIpAddressAndNullDescription_ReturnsSucces()
    {
        // Arrange
        var acc = new List<SambaAccount>();
        var mock = Setup(acc);

        var sut = new CreateSamba.Handler(mock.Object, new CreateSamba.Validator());
        var createCommand = new CreateSamba.Command()
        {
            Description = null,
            IPv4Address = "192.168.1.1" 
        };
        
        // Act
        var sambaAccount = await sut.Handle(createCommand, default);

        // Assert
        sambaAccount.IsSuccess.Should().BeTrue();
        mock.Verify(x => x.SambaAccounts.Add(It.IsAny<SambaAccount>()), Times.Once);
        mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    
    public async Task CreateSambaHandler_ValidData_ReturnsSucces()
    {
        // Arrange
        var acc = new List<SambaAccount>();
        var mock = Setup(acc);

        var sut = new CreateSamba.Handler(mock.Object, new CreateSamba.Validator());
        var createCommand = new CreateSamba.Command()
        {
            Description = "Samba account 2",
            IPv4Address = "192.168.1.1" 
        };
        
        // Act
        var sambaAccount = await sut.Handle(createCommand, default);

        // Assert
        sambaAccount.IsSuccess.Should().BeTrue();
        mock.Verify(x => x.SambaAccounts.Add(It.IsAny<SambaAccount>()), Times.Once);
        mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}