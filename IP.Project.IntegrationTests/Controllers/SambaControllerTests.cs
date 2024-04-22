using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.IntegrationTests.Base;
using Newtonsoft.Json;

namespace IP.Project.IntegrationTests.Controllers
{
    public class SambaControllerTests : BaseAppContextTests
    {
        private const string RequestUri = "/api/v1/sambas/";

        [Fact]
        public async Task When_GetSambaById_Exists_Then_Success()
        {
            // Arrange
            var existingSambaId = Guid.Parse("b1f5d163-ff83-411a-4144-08dc5ef3042e");

            // Act
            var response = await Client.GetAsync(RequestUri + existingSambaId);

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
            var response = await Client.GetAsync(RequestUri + nonExistingSambaId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}