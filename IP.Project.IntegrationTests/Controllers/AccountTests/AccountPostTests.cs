using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.IntegrationTests.Base;
using Newtonsoft.Json;

namespace IP.Project.IntegrationTests.Controllers;

public class AccountPostTests : BaseAppContextTests
{
    private const string RequestUri = "/api/v1/accounts";

    [Fact]
    public async Task When_PostAccountByIsCalledWithRightParameters_Then_Created()
    {
        // Arrange
        var request = new CreateAccountRequest()
        {
            Username = "Test account",
            Password = "password",
            Email = "test@test.com",
            Matricol = "123456789ABC123456",
        };

        // Act
        var response = await Client.PostAsJsonAsync(RequestUri, request);

        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<string>(responseString);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result?.Should().NotBeNull();
    }
}
