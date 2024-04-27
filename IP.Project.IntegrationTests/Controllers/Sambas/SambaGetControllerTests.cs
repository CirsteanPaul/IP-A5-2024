using System.Net;
using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.IntegrationTests.Base.TestingBaseWebApplicationFactory;
using IP.Project.Shared;
using Newtonsoft.Json;

namespace IP.Project.IntegrationTests.Controllers.Sambas
{
    public class SambaGetControllerTests : IClassFixture<TestingBaseWebApplicationFactory>
    {
        private static readonly string RequestUri = Global.version + "sambas/";
        private readonly TestingBaseWebApplicationFactory factory;

        public SambaGetControllerTests(TestingBaseWebApplicationFactory factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task When_GetSambaById_Exists_Then_Success()
        {
            // Arrange
            var existingSambaId = Guid.Parse("b1f5d163-ff83-411a-4144-08dc5ef3042e");

            // Act
            var response = await factory.Client.GetAsync(RequestUri + existingSambaId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();
            var sambaResponse = JsonConvert.DeserializeObject<SambaResponse>(responseString);
            sambaResponse.Should().NotBeNull();
        }

        [Fact]
        public async Task When_GetSambaById_DoesNotExist_Then_NotFound()
        {
            // Arrange
            Guid nonExistingSambaId = Guid.Parse("b1f5d163-ee13-411a-4144-07dc5ef3042e");

            // Act
            var response = await factory.Client.GetAsync(RequestUri + nonExistingSambaId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}