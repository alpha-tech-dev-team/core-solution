using System.Net;
using System.Net.Http.Json;
using Identity.Application.Contracts.Admin;
using Identity.Application.Contracts.Client;
using Identity.Domain.Enums;
using Xunit;

namespace CoreSolution.Integration.Tests;

[Collection(nameof(IntegrationCollection))]
public sealed class AdminEndpointsTests(IntegrationFixture fixture)
{
    private const string LoginRoute = "/api/admin/login";
    private const string UsersRoute = "/api/admin/users";
    private const string RolesRoute = "/api/admin/roles";

    [Fact]
    public async Task GetUsers_WithoutAuth_ReturnsUnauthorized()
    {
        using var client = fixture.CreateIdentityHttpClient();

        var response = await client.GetAsync(UsersRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_AsAdmin_ReturnsSeededUserWithRoles()
    {
        const string phone = "+79995550201";
        await fixture.SeedPendingUserAsync(phone, "Pandora");
        using var client = await CreateLoggedInAdminClient();

        var response = await client.GetAsync(UsersRoute);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await response.Content.ReadFromJsonAsync<EmployeeInfoDto[]>();
        var seeded = Assert.Single(users!, user => user.Phone == phone);
        Assert.Equal(nameof(UserStatus.Pending), seeded.Status);
        Assert.Contains("Pandora", seeded.Roles);
    }

    [Fact]
    public async Task GetRoles_AsAdmin_ReturnsSeededRoles()
    {
        await fixture.SeedPendingUserAsync("+79995550202", "Atlas");
        using var client = await CreateLoggedInAdminClient();

        var response = await client.GetAsync(RolesRoute);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var roles = await response.Content.ReadFromJsonAsync<string[]>();
        Assert.Contains("Atlas", roles!);
    }

    [Fact]
    public async Task SetRoles_UnknownRole_ReturnsNotFound()
    {
        const string phone = "+79995550203";
        await fixture.SeedPendingUserAsync(phone);
        using var client = await CreateLoggedInAdminClient();
        var users = await client.GetFromJsonAsync<EmployeeInfoDto[]>(UsersRoute);
        var seeded = Array.Find(users!, user => user.Phone == phone)!;

        var response = await client.PutAsJsonAsync($"{UsersRoute}/{seeded.Id}/roles", new SetUserRolesRequest(["NoSuchRole"]));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SetStatus_BlockedUser_LosesAccess()
    {
        const string phone = "+79995550204";
        const long telegramUserId = 9104;
        await fixture.SeedPendingUserAsync(phone);
        using var client = await CreateLoggedInAdminClient();
        var users = await client.GetFromJsonAsync<EmployeeInfoDto[]>(UsersRoute);
        var seeded = Array.Find(users!, user => user.Phone == phone)!;
        _ = await client.PostAsJsonAsync("/api/client/activate", new ActivateUserRequest(telegramUserId, phone));

        var response = await client.PutAsJsonAsync($"{UsersRoute}/{seeded.Id}/status", new SetUserStatusRequest("Blocked"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var (_, status) = await fixture.GetUserStateAsync(phone);
        Assert.Equal(UserStatus.Blocked, status);

        var check = await client.PostAsJsonAsync("/api/client/check", new CheckUserRequest(telegramUserId, "Atlas"));
        Assert.NotEqual(HttpStatusCode.OK, check.StatusCode);
    }

    [Fact]
    public async Task SetRoles_ExistingRole_ReplacesUserRoles()
    {
        const string phone = "+79995550205";
        await fixture.SeedPendingUserAsync(phone, "Pandora");
        using var client = await CreateLoggedInAdminClient();
        var users = await client.GetFromJsonAsync<EmployeeInfoDto[]>(UsersRoute);
        var seeded = Array.Find(users!, user => user.Phone == phone)!;

        var response = await client.PutAsJsonAsync($"{UsersRoute}/{seeded.Id}/roles", new SetUserRolesRequest(["Admin"]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var refreshed = await client.GetFromJsonAsync<EmployeeInfoDto[]>(UsersRoute);
        var updated = Array.Find(refreshed!, user => user.Phone == phone)!;
        Assert.Equal(["Admin"], updated.Roles);
    }

    private async Task<HttpClient> CreateLoggedInAdminClient()
    {
        var username = $"admin_{Guid.NewGuid():N}";
        await fixture.SeedAdminAsync(username, "admin-password-123");

        var client = fixture.CreateIdentityHttpClientWithCookies();
        var response = await client.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, "admin-password-123"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return client;
    }

    private sealed record EmployeeInfoDto(
        Guid Id,
        string Name,
        string Surname,
        string? Patronymic,
        string Position,
        string Phone,
        string Status,
        string[] Roles);
}
