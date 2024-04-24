using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IP.Project.Contracts;
using Newtonsoft.Json;
using Store.FunctionalTests;

namespace IP.Project.IntegrationTests.Controllers.AccountTests;

public class AccountPutTests : IClassFixture<TestingBaseWebApplicationFactory>
{
    private readonly TestingBaseWebApplicationFactory factory;
    private const string RequestUri = "/api/v1/accounts/update/";

    public AccountPutTests(TestingBaseWebApplicationFactory factory)
    {
        this.factory = factory;
    }

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
        var response = await factory.Client.PutAsJsonAsync(RequestUri + existingAccountId, request);

        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<string>(responseString);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result?.Should().NotBeNull();
    }
}