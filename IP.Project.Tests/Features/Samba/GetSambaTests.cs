using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using IP.Project.Database;
using IP.Project.Entities;
using IP.Project.Features.Samba;
using IP.Project.Shared;
using Moq;
using Moq.Dapper;
using NSubstitute;
using Xunit;

namespace IP.Project.Tests.Features.Samba
{
    public class GetSambaTests
    {
        private readonly Mock<ISqlConnectionFactory> _mockSqlConnectionFactory;
        private readonly Mock<IDbConnection> _mockDbConnection;
        private readonly GetSamba.Handler _handler;

        public GetSambaTests()
        {
            _mockSqlConnectionFactory = new Mock<ISqlConnectionFactory>();
            _mockDbConnection = new Mock<IDbConnection>();

            _mockSqlConnectionFactory.Setup(f => f.CreateConnection()).Returns(_mockDbConnection.Object);

            _handler = new GetSamba.Handler(_mockSqlConnectionFactory.Object);
        }

        [Fact]
        public async Task GetSambaHandler_ValidId_ReturnsSuccess()
        {
            // Arrange
            var accountId = Guid.Parse("4c727215-0522-4384-8481-4a2d1e094fb7");
            var sambaAccount = new SambaAccount { Id = accountId, Description = "Test Account", IPv4Address = "192.168.1.1" };

            // Configurarea mock-ului pentru Dapper
            _mockDbConnection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<SambaAccount>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                null,
                null,
                null)).ReturnsAsync(sambaAccount);

            var query = new GetSamba.Query { Id = accountId };

            // Act
            var result = await _handler.Handle(query, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(accountId);
            result.Value.Description.Should().Be("Test Account");
            result.Value.IPv4Address.Should().Be("192.168.1.1");
        }

        [Fact]
        public async Task GetSambaHandler_InvalidId_ReturnsFailure()
        {
            // Arrange
            var invalidId = Guid.Parse("b0af913e-4b78-408d-98b3-8103fb3b1870");

            // Configurarea mock-ului pentru Dapper
            _mockDbConnection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<SambaAccount>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                null,
                null,
                null)).ReturnsAsync((SambaAccount)null);

            var query = new GetSamba.Query { Id = invalidId };

            // Act
            var result = await _handler.Handle(query, default);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("GetSamba.Null");
            result.Error.Message.Should().Be("Samba not found");
        }
    }
}
