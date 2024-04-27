using FluentAssertions;
using IP.Project.Entities;
using IP.Project.Features.Samba;
using IP.Project.Tests.Base;

namespace IP.Project.Tests.Features.Samba
{
    public class GetSambaTests : BaseTest<SambaAccount>
    {
        [Fact]
        public async Task GetSambaHandler_ValidId_ReturnsSuccess()
        {
            // Arrange
            var accountId = Guid.Parse("4c727215-0522-4384-8481-4a2d1e094fb7");
            var sambaAccount = new SambaAccount { Id = accountId, Description = "Test Account", IPv4Address = "192.168.1.1" };
            var mock = Setup(new List<SambaAccount> { sambaAccount });

            var sut = new GetSamba.Handler(mock);
            var query = new GetSamba.Query { Id = accountId };

            // Act
            var result = await sut.Handle(query, default);

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
            var mock = Setup(new List<SambaAccount>());
            var sut = new GetSamba.Handler(mock);
            var query = new GetSamba.Query { Id = invalidId };

            // Act
            var result = await sut.Handle(query, default);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("GetSamba.Null");
            result.Error.Message.Should().Be("Samba not found");
        }
    }
}
