using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using IP.Project.IntegrationTests.Base;
using IP.Project.Shared;

namespace IP.Project.IntegrationTests.Controllers.Sambas
{
    public class SambaDeleteControllerTests : IClassFixture<TestingBaseWebApplicationFactory>
    {
        private readonly TestingBaseWebApplicationFactory _factory;
        private readonly string _requestUri = $"{Global.version}sambas/";

        private readonly Guid _existingSambaId = Guid.Parse("b1f5d163-ff83-411a-4144-08dc5ef3042e");
        private readonly Guid _nonExistingSambaId = Guid.Parse("b1f5d163-aa23-411a-4144-08dc5ef3042e");

        public SambaDeleteControllerTests(TestingBaseWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task When_DeleteSamba_WithExistingId_Then_ReturnsNoContent()
        {
            // Arrange
            _factory.Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestingBaseWebApplicationFactory.CreateAdminToken());

            // Act
            var response = await _factory.Client.DeleteAsync(_requestUri + _existingSambaId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task When_DeleteSamba_WithNonExistingId_Then_ReturnsNotFound()
        {
            // Arrange
            _factory.Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestingBaseWebApplicationFactory.CreateAdminToken());

            // Act
            var response = await _factory.Client.DeleteAsync(_requestUri + _nonExistingSambaId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}