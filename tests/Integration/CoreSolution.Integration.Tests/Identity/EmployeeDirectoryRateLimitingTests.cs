using System.Net;
using CoreSolution.Shared.Constants;
using Identity.Api.Constants;
using Identity.Api.Controllers;
using Identity.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Xunit;

namespace CoreSolution.Integration.Tests.IdentityTests;

[Collection(nameof(IntegrationCollection))]
public sealed class EmployeeDirectoryRateLimitingTests(IntegrationFixture fixture)
{
    [Fact]
    public void Policy_PartitionsRequestsByTelegramUserId()
    {
        var policy = new EmployeeDirectoryRateLimitingPolicy(Options.Create(new EmployeeDirectoryRateLimitingOptions()));

        var first = policy.GetPartition(CreateContext("1001"));
        var sameUser = policy.GetPartition(CreateContext("1001"));
        var anotherUser = policy.GetPartition(CreateContext("2002"));

        Assert.Equal(first.PartitionKey, sameUser.PartitionKey);
        Assert.NotEqual(first.PartitionKey, anotherUser.PartitionKey);
    }

    [Fact]
    public void EmployeeRoute_UsesPartitionedEmployeeDirectoryPolicy()
    {
        Assert.Equal(
            RateLimitingPolicies.EmployeeDirectory,
            GetPolicyName(nameof(ClientController.GetEmployee)));
    }

    [Fact]
    public void BotRoutes_UseSeparateBotClientPolicy()
    {
        Assert.Equal(
            RateLimitingPolicies.BotClient,
            GetPolicyName(nameof(ClientController.ActivateUser)));

        Assert.Equal(
            RateLimitingPolicies.BotClient,
            GetPolicyName(nameof(ClientController.CheckUser)));
    }

    [Fact]
    public async Task EmployeeDirectory_LimitsRequestsPerTelegramUserId()
    {
        using var client = fixture.CreateRateLimitedIdentityHttpClient();

        var first = await client.GetAsync(ClientApiRoutes.EmployeeByTelegramUser(9101));
        var second = await client.GetAsync(ClientApiRoutes.EmployeeByTelegramUser(9101));
        var limited = await client.GetAsync(ClientApiRoutes.EmployeeByTelegramUser(9101));
        var anotherUser = await client.GetAsync(ClientApiRoutes.EmployeeByTelegramUser(9102));

        Assert.NotEqual(HttpStatusCode.TooManyRequests, first.StatusCode);
        Assert.NotEqual(HttpStatusCode.TooManyRequests, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.NotEqual(HttpStatusCode.TooManyRequests, anotherUser.StatusCode);
    }

    private static string? GetPolicyName(string actionName)
    {
        var action = typeof(ClientController).GetMethod(actionName);

        Assert.NotNull(action);

        var attribute = action!
            .GetCustomAttributes(typeof(EnableRateLimitingAttribute), inherit: true)
            .Cast<EnableRateLimitingAttribute>()
            .SingleOrDefault();

        Assert.NotNull(attribute);

        return attribute!.PolicyName;
    }

    private static DefaultHttpContext CreateContext(string telegramUserId)
    {
        var context = new DefaultHttpContext();

        context.Request.RouteValues["telegramUserId"] = telegramUserId;

        return context;
    }
}
