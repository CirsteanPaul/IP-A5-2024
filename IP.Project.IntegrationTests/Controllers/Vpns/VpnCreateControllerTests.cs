using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using IP.Project.Contracts.Vpn;
using IP.Project.IntegrationTests.Base;

namespace IP.Project.IntegrationTests.Controllers.Vpns
{
    public class VpnCreateControllerTests : IClassFixture<TestingBaseWebApplicationFactory>
    {
        private const string RequestUri = "/api/v1/vpns/";
        private readonly TestingBaseWebApplicationFactory factory;

        public VpnCreateControllerTests(TestingBaseWebApplicationFactory factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task When_CreateVpn_WithValidData_Then_ReturnsCreatedResponseAndCorrectData()
        {
            // Arrange
            var vpnRequest = new CreateVpnRequest
            {
                Description = "New VPN",
                IPv4Address = "192.168.1.1"
            };
            factory.Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestingBaseWebApplicationFactory.CreateAdminToken());

            // Act
            var response = await factory.Client.PostAsJsonAsync(RequestUri, vpnRequest);

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
            factory.Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestingBaseWebApplicationFactory.CreateAdminToken());

            // Act
            var response = await factory.Client.PostAsJsonAsync(RequestUri, invalidVpnRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}