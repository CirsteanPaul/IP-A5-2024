﻿using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IP.Project.Contracts;
using IP.Project.IntegrationTests.Base.TestingBaseWebApplicationFactory;
using IP.Project.IntegrationTests.Base;
using Newtonsoft.Json;
using IP.Project.Features.Accounts;

namespace IP.Project.IntegrationTests.Controllers.AccountTests;

public class AccountPutControllerTests : IClassFixture<TestingBaseWebApplicationFactory>
{
    private readonly TestingBaseWebApplicationFactory factory;
    private const string RequestUri = Global.versionAccount;

    public AccountPutControllerTests(TestingBaseWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task When_PutAccountById_Exists_AndChangedUsername_Then_Success()
    {
        // Arrange
        var existingAccountId = Guid.Parse("f1302aae-193b-4ae0-8d1d-b8a3f239fff1");
        var request = new UpdateAccountRequest()
        {
            NewUsername = "New username"
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