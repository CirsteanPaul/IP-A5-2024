﻿using System.Net;
using FluentAssertions;
using IP.Project.IntegrationTests.Base;
using IP.Project.Shared;
using Newtonsoft.Json;

namespace IP.Project.IntegrationTests.Controllers.Accounts;

public class AccountDeleteControllerTests : IClassFixture<TestingBaseWebApplicationFactory>
{
    private readonly TestingBaseWebApplicationFactory factory;
    private const string RequestUri = Global.VersionAccount;

    public AccountDeleteControllerTests(TestingBaseWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task When_DeleteAccountById_Exists_Then_NoContent()
    {
        // Arrange
        var existingAccountId = Guid.Parse("f1302aae-193b-4ae0-8d1d-b8a3f239fff1");

        // Act
        var response = await factory.Client.DeleteAsync(RequestUri + existingAccountId);

        response.EnsureSuccessStatusCode();
        var responseString = response.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<string>(responseString);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        result?.Should().NotBeNull();
    }

    [Fact]
    public async Task When_DeleteAccountById_DoesNotExist_Then_NotFound()
    {
        // Arrange
        var existingAccountId = Guid.Parse("828cccf2-4c62-4241-836f-4253b3ebb321");

        // Act
        var response = await factory.Client.DeleteAsync(RequestUri + existingAccountId);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
