using System.Net;
using FluentAssertions;
using Newtonsoft.Json;
using Store.FunctionalTests;

namespace IP.Project.IntegrationTests.Controllers.AccountTests;

public class AccountDeleteTests : IClassFixture<TestingBaseWebApplicationFactory>
{
    private readonly TestingBaseWebApplicationFactory factory;
    private const string RequestUri = "/api/v1/accounts/";

    public AccountDeleteTests(TestingBaseWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public void When_DeleteAccountById_Exists_Then_NoContent()
    {
        // Arrange
        var existingAccountId = Guid.Parse("b5ec0aed-e1f4-4115-80e4-e4448b1f43ab");

        // Act
        var response = factory.Client.DeleteAsync(RequestUri + existingAccountId).Result;

        response.EnsureSuccessStatusCode();
        var responseString = response.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<string>(responseString);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result?.Should().NotBeNull();
    }
}
