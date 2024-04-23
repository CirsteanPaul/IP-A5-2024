using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.IntegrationTests.Base;
using Newtonsoft.Json;

namespace IP.Project.IntegrationTests.Controllers;

public class AccountDeleteTests : BaseAppContextTests
{
    private const string RequestUri = "/api/v1/accounts/";

    [Fact]
    public async Task When_DeleteAccountById_Exists_Then_NoContent()
    {
        // Arrange
        var existingAccountId = Guid.Parse("b5ec0aed-e1f4-4115-80e4-e4448b1f43ab");

        // Act
        var response = await Client.DeleteAsync(RequestUri + existingAccountId);

        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<string>(responseString);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result?.Should().NotBeNull();
    }
}
