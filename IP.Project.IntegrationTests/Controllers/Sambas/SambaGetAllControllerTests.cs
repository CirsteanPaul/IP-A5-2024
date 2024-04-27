using System.Net;
using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.IntegrationTests.Base.TestingBaseWebApplicationFactory;
using IP.Project.Shared;
using Newtonsoft.Json;

namespace IP.Project.IntegrationTests.Controllers.Sambas;

public class SambaGetAllControllerTests : IClassFixture<TestingBaseWebApplicationFactory>
{
    private static readonly string RequestUri = Global.version + "sambas/";
    private readonly TestingBaseWebApplicationFactory factory;

    public SambaGetAllControllerTests(TestingBaseWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task When_GetAllSambas_Then_Success()
    {
        // Arrange

        // Act
        var response = await factory.Client.GetAsync(RequestUri);
        var responseString = await response.Content.ReadAsStringAsync();
        var sambasResponse = JsonConvert.DeserializeObject<List<SambaResponse>>(responseString);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        sambasResponse.Should().HaveCount(2);
    }
}