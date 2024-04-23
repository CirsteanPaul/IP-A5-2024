using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.IntegrationTests.Base;

namespace IP.Project.IntegrationTests.Controllers
{
    public class GetAllVpnsControllerTests : BaseAppContextTests
    {
        private const string RequestUri = "/api/v1/vpns/";

        [Fact]
        public async Task GetAllVpns_ReturnsListOfVpns_WhenExists()
        {
            // Arrange 
            var existingid = Guid.Parse("b1f5d163-ff83-411a-4144-08dc5ef3042e");
            
            // Act
            var response = await Client.GetAsync(RequestUri);
            var vpns = await response.Content.ReadFromJsonAsync<List<VpnResponse>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            vpns.Should().NotBeNull();
            vpns.Should().HaveCount(2);
            vpns.Should().ContainSingle(x => x.Id == existingid);
        }
        
    }
}