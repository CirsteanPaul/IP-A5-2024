using Xunit;
using Moq;
using IP.Project.Features.Samba;
using IP.Project.Contracts;
using IP.Project.Shared;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using Moq.Dapper;
using IP.Project.Entities;

public class GetSambaTests
{
    private readonly Mock<IDbConnection> _mockDbConnection;
    private readonly GetSamba.Handler _handler;

    public GetSambaTests()
    {
        _mockDbConnection = new Mock<IDbConnection>();
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(config => config.GetConnectionString("DefaultConnection"))
            .Returns("YourConnectionString");

        _handler = new GetSamba.Handler(mockConfiguration.Object)
        {
            Connection = _mockDbConnection.Object
        };
    }

    [Fact]
    public async Task Handle_ReturnsSambaResponse_WhenSambaExists()
    {
        // Arrange
        var query = new GetSamba.Query { Id = Guid.NewGuid() };
        var expectedSamba = new SambaAccount
        {
            Id = query.Id,
            Description = "Test Description",
            IPv4Address = "127.0.0.1"
        };

        // Setup Dapper mock
        _mockDbConnection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<SambaAccount>(It.IsAny<string>(), null, null, null, null))
            .ReturnsAsync(expectedSamba);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(expectedSamba.Id, result.Value.Id);
        Assert.Equal(expectedSamba.Description, result.Value.Description);
        Assert.Equal(expectedSamba.IPv4Address, result.Value.IPv4Address);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenSambaDoesNotExist()
    {
        // Arrange
        var query = new GetSamba.Query { Id = Guid.NewGuid() };

        // Setup Dapper mock to return null
        _mockDbConnection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<SambaAccount>(It.IsAny<string>(), null, null, null, null))
            .ReturnsAsync((SambaAccount)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Null(result.Value);
    }
}
