using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.IntegrationTests.Base.TestingBaseWebApplicationFactory;
using IP.Project.Shared;


namespace IP.Project.IntegrationTests.Controllers.Vpns
{
    public class VpnUpdateControllerTests : IClassFixture<TestingBaseWebApplicationFactory>

    {
        private readonly TestingBaseWebApplicationFactory factory;
        private const string RequestUri = Global.version + "vpns/";

        public VpnUpdateControllerTests(TestingBaseWebApplicationFactory factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task When_UpdateVpn_WithValidData_Then_ReturnsSuccess()
        {
            // Arrange
            var existingVpnId = Guid.Parse("2330d4f5-1c5b-42cb-a34b-d9275e99b6bc");
            var updateRequest = new UpdateVpnRequest
            {
                NewDescription = "Updated VPN Description",
                NewIpAddress = "192.168.100.1"
            };

            // Act
            var response = await factory.Client.PutAsJsonAsync($"{RequestUri}{existingVpnId}", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
        
        [Fact]
        public async Task When_UpdateVpn_ThatDoesNotExist_Then_ReturnsNotFound()
        {
            // Arrange
            var nonExistingVpnId = Guid.Parse("deadbeef-1c5b-42cb-a34b-d9275e99b6bc");
            var updateRequest = new UpdateVpnRequest()
            {
                NewDescription = "Non-existent VPN Description",
                NewIpAddress = "192.168.100.2"
            };

            // Act
            var response = await factory.Client.PutAsJsonAsync($"{RequestUri}{nonExistingVpnId}", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}


