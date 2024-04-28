using System.Net;
using System.Text;
using FluentAssertions;
using IP.Project.Features.Samba;
using IP.Project.IntegrationTests.Base.TestingBaseWebApplicationFactory;
using IP.Project.Shared;
using Newtonsoft.Json;

namespace IP.Project.IntegrationTests.Controllers.Sambas
{
    public class SambaUpdateControllerTests : IClassFixture<TestingBaseWebApplicationFactory>
    {
        private static readonly string RequestUri = Global.version + "sambas/";
        private readonly TestingBaseWebApplicationFactory factory;
        public SambaUpdateControllerTests(TestingBaseWebApplicationFactory factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task When_UpdateSamba_Exists_Then_Success()
        {
            // Arrange
            var existingSambaId = Guid.Parse("b1f5d163-ff83-411a-4144-08dc5ef3042e");
            var updateRequest = new UpdateSambaRequest
            {
                NewIpAddress = "192.168.1.101",
                NewDescription = "New description"
            };
            var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");
            // Act
            var response = await factory.Client.PutAsync(RequestUri + existingSambaId, content);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task When_UpdateSamba_DoesNotExist_Then_NotFound()
        {
            // Arrange
            var nonExistingSambaId = Guid.Parse("b1f5d163-ee13-411a-4144-07dc5ef3042e");
            var updateRequest = new UpdateSambaRequest
            {
                NewIpAddress = "192.168.1.101",
                NewDescription = "New description"
            };
            var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");

            // Act
            var response = await factory.Client.PutAsync(RequestUri + nonExistingSambaId, content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
}
}
