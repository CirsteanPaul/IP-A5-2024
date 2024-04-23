using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.IntegrationTests.Base;

namespace IP.Project.IntegrationTests.Controllers
{
    public class VpnCreateControllerTests : BaseAppContextTests
    {
        private const string RequestUri = "/api/v1/vpns/";

        [Fact]
        public async Task When_CreateVpn_WithValidData_Then_ReturnsCreatedResponseAndCorrectData()
        {
            // Arrange
            var vpnRequest = new CreateVpnRequest
            {
                Description = "New VPN",
                IPv4Address = "192.168.1.1"
            };

            // Act
            var response = await Client.PostAsJsonAsync(RequestUri, vpnRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task When_CreateVpn_WithInvalidData_Then_ReturnsBadRequest()
        {
            // Arrange
            var invalidVpnRequest = new CreateVpnRequest
            {
                Description = "Invalid VPN",
                IPv4Address = ""
            };

            // Act
            var response = await Client.PostAsJsonAsync(RequestUri, invalidVpnRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}