using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.IntegrationTests.Base;
using Newtonsoft.Json;

namespace IP.Project.IntegrationTests.Controllers;

public class SambaCrudControllerExample : BaseAppContextTests
{
    private const string RequestUri = "/api/samba";

    [Fact]
    public async Task When_PostSambaByIsCalledWithRightParameters_Then_Success()
    {
        // Arrange
        var request = new CreateSambaRequest()
        {
            Description = "Test samba",
            IPv4Address = "102.240.100.100"
        };
        
        // Act
        var response = await Client.PostAsJsonAsync(RequestUri, request);

        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<string>(responseString);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result?.Should().NotBeNull();
    }
}