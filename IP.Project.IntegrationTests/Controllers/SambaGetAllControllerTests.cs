using System.Net;
using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.IntegrationTests.Base;
using Newtonsoft.Json;

namespace IP.Project.IntegrationTests.Controllers;

public class SambaGetAllControllerTests : BaseAppContextTests
{
    private const string RequestUri = "/api/v1/sambas/";

    [Fact]
    public async Task When_GetAllSambas_Then_Success()
    {
        // Arrange

        // Act
        var response = await Client.GetAsync(RequestUri);
        var responseString = await response.Content.ReadAsStringAsync();
        var sambasResponse = JsonConvert.DeserializeObject<List<SambaResponse>>(responseString);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        sambasResponse.Should().HaveCount(2);
    }
}