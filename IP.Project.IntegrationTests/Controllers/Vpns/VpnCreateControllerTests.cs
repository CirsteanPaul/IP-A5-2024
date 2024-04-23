using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.Database;
using IP.Project.IntegrationTests.Base;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

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

            var returnedVpn = JsonConvert.DeserializeObject<VpnResponse>(await response.Content.ReadAsStringAsync());
            returnedVpn.Should().NotBeNull();
            returnedVpn.Description.Should().Be(vpnRequest.Description);
            returnedVpn.IPv4Address.Should().Be(vpnRequest.IPv4Address);

            using (var scope = Application.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDBContext>();
                var vpnInDb = await db.Vpns.FindAsync(returnedVpn.Id);
                vpnInDb.Should().NotBeNull();
                vpnInDb.Description.Should().Be(vpnRequest.Description);
                vpnInDb.IPv4Address.Should().Be(vpnRequest.IPv4Address);
            }
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