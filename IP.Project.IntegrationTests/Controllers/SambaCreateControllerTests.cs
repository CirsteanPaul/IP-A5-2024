using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.IntegrationTests.Base;

namespace IP.Project.IntegrationTests.Controllers;

public class SambaCreateControllerTests : BaseAppContextTests
{
    private const string RequestUri = "/api/v1/sambas";  

    [Fact]
    public async Task When_CreateSambaWithValidIP_Then_ReturnsCreated()
    {
        // Arrange
        var request = new CreateSambaRequest()
        {
            Description = "New Samba",
            IPv4Address = "192.168.1.100" 
        };

        // Act
        var response = await Client.PostAsJsonAsync(RequestUri, request);

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

        // Act
        var response = await Client.PostAsJsonAsync(RequestUri, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

