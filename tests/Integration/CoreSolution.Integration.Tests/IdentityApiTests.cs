using System.Net;
using System.Net.Http.Json;
using Identity.Application.Contracts.Client;
using Xunit;

namespace CoreSolution.Integration.Tests;

[Collection(nameof(IntegrationCollection))]
public sealed class IdentityApiTests(IntegrationFixture fixture)
{
    private const string ActivateRoute = "/api/client/activate";
    private const string CheckRoute = "/api/client/check";

    [Fact]
    public async Task Activate_KnownPendingPhone_ReturnsSuccess()
    {
        const string phone = "+79995550001";

        await fixture.SeedPendingUserAsync(phone);
        using var client = fixture.CreateIdentityHttpClient();

        var response = await client.PostAsJsonAsync(ActivateRoute, new ActivateUserRequest(9001, phone));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Activate_UnknownPhone_ReturnsFailure()
    {
        using var client = fixture.CreateIdentityHttpClient();

        var response = await client.PostAsJsonAsync(ActivateRoute, new ActivateUserRequest(9002, "+79995550002"));

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Check_ActivatedUserWithMatchingRole_ReturnsSuccess()
    {
        const string phone = "+79995550003";
        const long telegramUserId = 9003;

        await fixture.SeedPendingUserAsync(phone, "Pandora");
        using var client = fixture.CreateIdentityHttpClient();
        _ = await client.PostAsJsonAsync(ActivateRoute, new ActivateUserRequest(telegramUserId, phone));

        var response = await client.PostAsJsonAsync(CheckRoute, new CheckUserRequest(telegramUserId, "pandora"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Check_ActivatedUserWithoutRole_ReturnsFailure()
    {
        const string phone = "+79995550004";
        const long telegramUserId = 9004;

        await fixture.SeedPendingUserAsync(phone, "Atlas");
        using var client = fixture.CreateIdentityHttpClient();
        _ = await client.PostAsJsonAsync(ActivateRoute, new ActivateUserRequest(telegramUserId, phone));

        var response = await client.PostAsJsonAsync(CheckRoute, new CheckUserRequest(telegramUserId, "Pandora"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
