using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.IntegrationTests.Base;
using Newtonsoft.Json;

namespace IP.Project.IntegrationTests.Controllers;

public class AccountPutTests : BaseAppContextTests
{
    private const string RequestUri = "/api/v1/accounts/update/";

    [Fact]
    public async Task When_PutAccountById_Exists_AndChangedUsername_Then_Success()
    {
        // Arrange
        var existingAccountId = Guid.Parse("b5ec0aed-e1f4-4115-80e4-e4448b1f43ab");
        var request = new CreateAccountRequest()
        {
            Username = "New username",
            Password = "",
            Email = "",
            Matricol = "",
        };

        // Act
        var response = await Client.PutAsJsonAsync(RequestUri + existingAccountId, request);

        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<string>(responseString);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result?.Should().NotBeNull();
    }
}