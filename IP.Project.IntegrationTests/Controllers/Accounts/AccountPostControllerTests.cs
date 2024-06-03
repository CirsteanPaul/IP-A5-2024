using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IP.Project.Contracts.Account;
using IP.Project.IntegrationTests.Base;
using IP.Project.Shared;
using Newtonsoft.Json;

namespace IP.Project.IntegrationTests.Controllers.Accounts;

public class AccountPostControllerTests : IClassFixture<TestingBaseWebApplicationFactory>
{
    private readonly TestingBaseWebApplicationFactory factory;
    private const string RequestUri = Global.VersionAccount;

    public AccountPostControllerTests(TestingBaseWebApplicationFactory factory)
    {
        this.factory = factory;
    }

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
        var response = await factory.Client.PostAsJsonAsync(RequestUri, request);

        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<string>(responseString);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result?.Should().NotBeNull();
    }

    [Fact]
    public async Task When_PostAccountByIsCalledWithInvalidEmail_Then_BadRequest()
    {
        // Arrange
        var request = new CreateAccountRequest()
        {
            Username = "Test account",
            Password = "password",
            Email = "test",
            Matricol = "123456789ABC123456",
        };

        // Act
        var response = await factory.Client.PostAsJsonAsync(RequestUri, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task When_PostAccountByIsCalledWithInvalidMatricol_Then_BadRequest()
    {
        // Arrange
        var request = new CreateAccountRequest()
        {
            Username = "Test account",
            Password = "password",
            Email = "test@test.com",
            Matricol = "123456789123456",
        };

        // Act
        var response = await factory.Client.PostAsJsonAsync(RequestUri, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task When_PostAccountByIsCalledWithEmptyEmail_Then_BadRequest()
    {
        // Arrange
        var request = new CreateAccountRequest()
        {
            Username = "Test account",
            Password = "password",
            Matricol = "123456789123456",
        };

        // Act
        var response = await factory.Client.PostAsJsonAsync(RequestUri, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task When_PostAccountByIsCalledWithEmptyUsername_Then_BadRequest()
    {
        // Arrange
        var request = new CreateAccountRequest()
        {
            Username = "",
            Password = "password",
            Email = "test@test.com",
            Matricol = "123456789123456",
        };

        // Act
        var response = await factory.Client.PostAsJsonAsync(RequestUri, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
