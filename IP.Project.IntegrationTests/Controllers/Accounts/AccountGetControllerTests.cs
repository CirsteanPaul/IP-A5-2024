using System.Net;
using FluentAssertions;
using IP.Project.Contracts.Account;
using IP.Project.IntegrationTests.Base;
using IP.Project.Shared;
using Newtonsoft.Json;

namespace IP.Project.IntegrationTests.Controllers.Accounts;

public class AccountGetControllerTests : IClassFixture<TestingBaseWebApplicationFactory>
{
    private readonly TestingBaseWebApplicationFactory factory;
    private const string RequestUri = Global.versionAccount;

    public AccountGetControllerTests(TestingBaseWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task When_GetAccountById_Exists_Then_Success()
    {
        // Arrange
        var existingAccountId = Guid.Parse("f1302aae-193b-4ae0-8d1d-b8a3f239fff1");

        // Act
        var response = await factory.Client.GetAsync(RequestUri + existingAccountId);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseString = await response.Content.ReadAsStringAsync();
        var accountResponse = JsonConvert.DeserializeObject<AccountResponse>(responseString);
        accountResponse.Should().NotBeNull();
    }

    [Fact]
    public async Task When_GetAccountById_DoesNotExist_Then_NotFound()
    {
        // Arrange
        Guid nonExistingAccountId = Guid.Parse("828cccf2-4c62-4241-836f-4253b3ebb321");

        // Act
        var response = await factory.Client.GetAsync(RequestUri + nonExistingAccountId);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
