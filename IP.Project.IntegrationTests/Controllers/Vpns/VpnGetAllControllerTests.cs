using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using IP.Project.Contracts.Vpn;
using IP.Project.IntegrationTests.Base;

namespace IP.Project.IntegrationTests.Controllers.Vpns
{
    public class GetAllVpnsControllerTests : IClassFixture<TestingBaseWebApplicationFactory>
    {
        private const string RequestUri = "/api/v1/vpns/";
        private readonly TestingBaseWebApplicationFactory factory;

        public GetAllVpnsControllerTests(TestingBaseWebApplicationFactory factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task GetAllVpns_ReturnsListOfVpns_WhenExists()
        {
            // Arrange 
            var existingId = Guid.Parse("2330d4f5-1c5b-42cb-a34b-d9275e99b6bc");
            factory.Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestingBaseWebApplicationFactory.CreateUserToken());
            
            // Act
            var response = await factory.Client.GetAsync(RequestUri);
            var vpns = await response.Content.ReadFromJsonAsync<List<VpnResponse>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            vpns.Should().NotBeNull();
            vpns.Should().HaveCount(2);
            vpns.Should().ContainSingle(x => x.Id == existingId);
        }
    }
}