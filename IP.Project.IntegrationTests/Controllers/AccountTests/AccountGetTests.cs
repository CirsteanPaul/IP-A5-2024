﻿using System.Net;
using FluentAssertions;
using IP.Project.Contracts;
using Newtonsoft.Json;
using Store.FunctionalTests;

namespace IP.Project.IntegrationTests.Controllers.AccountTests;

public class AccountGetTests : IClassFixture<TestingBaseWebApplicationFactory>
{
    private readonly TestingBaseWebApplicationFactory tests;
    private const string RequestUri = "/api/v1/accounts/";

    public AccountGetTests(TestingBaseWebApplicationFactory tests)
    {
        this.tests = tests;
    }

    [Fact]
    public async Task When_GetAccountById_Exists_Then_Success()
    {
        // Arrange
        var existingAccountId = Guid.Parse("b5ec0aed-e1f4-4115-80e4-e4448b1f43ab");

        // Act
        var response = await tests.Client.GetAsync(RequestUri + existingAccountId);

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
        Guid nonExistingAccountId = Guid.Parse("b1f5d163-0013-411a-41aa-07dc5ef3042e");

        // Act
        var response = await tests.Client.GetAsync(RequestUri + nonExistingAccountId);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
