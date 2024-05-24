using System.Net;
using FluentAssertions;
using IP.Project.IntegrationTests.Base;

namespace IP.Project.IntegrationTests.Controllers.Vpns
{
    public class VpnDeleteControllerTests : IClassFixture<TestingBaseWebApplicationFactory>
    {
        private const string RequestUri = "/api/v1/vpns/";

        private readonly TestingBaseWebApplicationFactory factory;

        public VpnDeleteControllerTests(TestingBaseWebApplicationFactory factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task When_DeleteVpn_Exists_Then_Success()
        {
            // Arrange
            var existingVpnId = Guid.Parse("2330d4f5-1c5b-42cb-a34b-d9275e99b6bc");

            // Act
            var response = await factory.Client.DeleteAsync($"{RequestUri}{existingVpnId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task When_DeleteVpn_DoesNotExist_Then_NotFound()
        {
            // Arrange
            var nonExistingVpnId = Guid.Parse("b1f5d163-ee13-411a-4144-07dc5ef3042e");

            // Act
            var response = await factory.Client.DeleteAsync($"{RequestUri}{nonExistingVpnId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}