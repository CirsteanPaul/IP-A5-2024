using System.Net;
using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.IntegrationTests.Base;
using Newtonsoft.Json;

namespace IP.Project.IntegrationTests.Controllers
{
    public class VpnGetControllerTests : BaseAppContextTests
    {
        private const string RequestUri = "/api/v1/vpns/";

        [Fact]
        public async Task When_GetVpnById_Exists_Then_Success()
        {
            // Arrange
            var existingVpnId = Guid.Parse("b1f5d163-ff83-411a-4144-08dc5ef3042e"); 

            // Act
            var response = await Client.GetAsync(RequestUri + existingVpnId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();
            var vpnResponse = JsonConvert.DeserializeObject<VpnResponse>(responseString);
            vpnResponse.Should().NotBeNull();
            vpnResponse.Id.Should().Be(existingVpnId);
        }

        [Fact]
        public async Task When_GetVpnById_DoesNotExist_Then_NotFound()
        {
            // Arrange
            var nonExistingVpnId = Guid.Parse("b1f5d163-ee13-411a-4144-07dc5ef3042e"); 

            // Act
            var response = await Client.GetAsync(RequestUri + nonExistingVpnId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}