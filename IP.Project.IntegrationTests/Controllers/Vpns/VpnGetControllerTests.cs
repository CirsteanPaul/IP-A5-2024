using System.Net;
using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.IntegrationTests.Base;
using Newtonsoft.Json;
using Store.FunctionalTests;

namespace IP.Project.IntegrationTests.Controllers.Vpns
{
    public class VpnGetControllerTests : IClassFixture<TestingBaseWebApplicationFactory>
    {
        private const string RequestUri = "/api/v1/vpns/";
        private readonly TestingBaseWebApplicationFactory factory;

        public VpnGetControllerTests(TestingBaseWebApplicationFactory factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task When_GetVpnById_Exists_Then_Success()
        {
            // Arrange
            var existingVpnId = Guid.Parse("2330d4f5-1c5b-42cb-a34b-d9275e99b6bc"); 

            // Act
            var response = await factory.Client.GetAsync(RequestUri + existingVpnId);

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
            var response = await factory.Client.GetAsync(RequestUri + nonExistingVpnId);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}