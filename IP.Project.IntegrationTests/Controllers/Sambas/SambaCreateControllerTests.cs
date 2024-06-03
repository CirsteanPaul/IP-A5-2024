using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using IP.Project.Contracts.Samba;
using IP.Project.IntegrationTests.Base;
using IP.Project.Shared;

namespace IP.Project.IntegrationTests.Controllers.Sambas;

public class SambaCreateControllerTests : IClassFixture<TestingBaseWebApplicationFactory>
{
    private static readonly string RequestUri = Global.version + "sambas/";
    private readonly TestingBaseWebApplicationFactory factory;

    public SambaCreateControllerTests(TestingBaseWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task When_CreateSambaWithValidIP_Then_ReturnsCreated()
    {
        // Arrange
        var request = new CreateSambaRequest()
        {
            Description = "New Samba long",
            IPv4Address = "192.168.1.100" 
        };

        // Act
        factory.Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestingBaseWebApplicationFactory.CreateAdminToken());
        var response = await factory.Client.PostAsJsonAsync(RequestUri, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task When_CreateSambaWithInvalidIP_Then_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateSambaRequest()
        {
            Description = "Invalid Samba",
            IPv4Address = "999.999.999.999"  
        };
        
        factory.Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestingBaseWebApplicationFactory.CreateAdminToken());

        // Act
        var response = await factory.Client.PostAsJsonAsync(RequestUri, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

