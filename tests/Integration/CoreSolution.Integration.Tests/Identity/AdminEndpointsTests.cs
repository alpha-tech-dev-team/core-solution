using System.Net;
using System.Net.Http.Json;
using Identity.Api.Models;
using Identity.Application.Contracts.Admin;
using Identity.Application.Contracts.Client;
using Identity.Application.Errors;
using Identity.Domain.Enums;
using Xunit;

namespace CoreSolution.Integration.Tests.IdentityTests;

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
    public async Task Refresh_WithoutCookie_ReturnsUnauthorizedWithErrorCode()
    {
        using var client = fixture.CreateIdentityHttpClient();

        var response = await client.PostAsync("/api/admin/refresh", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.InvalidRefreshToken), error!.Code);
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

    [Fact]
    public async Task CreateUser_AsAdmin_CreatesPendingEmployee()
    {
        const string phone = "+79995550206";
        using var client = await CreateLoggedInAdminClient();

        var response = await client.PostAsJsonAsync(
            UsersRoute,
            new UserDataRequest("Иван", "Иванов", "Иванович", "Инженер", phone));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await client.GetFromJsonAsync<EmployeeInfoDto[]>(UsersRoute);
        var created = Assert.Single(users!, user => user.Phone == phone);
        Assert.Equal("Иванов", created.Surname);
        Assert.Equal("Иванович", created.Patronymic);
        Assert.Equal("Инженер", created.Position);
        Assert.Equal(nameof(UserStatus.Pending), created.Status);
        Assert.Empty(created.Roles);
    }

    [Fact]
    public async Task CreateUser_DuplicatePhone_ReturnsConflict()
    {
        const string phone = "+79995550207";
        await fixture.SeedPendingUserAsync(phone);
        using var client = await CreateLoggedInAdminClient();

        var response = await client.PostAsJsonAsync(
            UsersRoute,
            new UserDataRequest("Иван", "Иванов", null, "Инженер", phone));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_InvalidData_ReturnsBadRequest()
    {
        using var client = await CreateLoggedInAdminClient();

        var response = await client.PostAsJsonAsync(
            UsersRoute,
            new UserDataRequest(string.Empty, "Иванов", null, "Инженер", "+79995550208"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.ValidationFailed), error!.Code);
    }

    [Fact]
    public async Task CreateUser_JunkValues_ReturnsBadRequest()
    {
        using var client = await CreateLoggedInAdminClient();

        var response = await client.PostAsJsonAsync(
            UsersRoute,
            new UserDataRequest("а", "а", null, "а", "а"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetStatus_UnblockNeverActivatedUser_RestoresPending()
    {
        const string phone = "+79995550213";
        await fixture.SeedPendingUserAsync(phone);
        using var client = await CreateLoggedInAdminClient();
        var users = await client.GetFromJsonAsync<EmployeeInfoDto[]>(UsersRoute);
        var seeded = Array.Find(users!, user => user.Phone == phone)!;

        _ = await client.PutAsJsonAsync($"{UsersRoute}/{seeded.Id}/status", new SetUserStatusRequest("Blocked"));
        var response = await client.PutAsJsonAsync($"{UsersRoute}/{seeded.Id}/status", new SetUserStatusRequest("Active"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var (_, status) = await fixture.GetUserStateAsync(phone);
        Assert.Equal(UserStatus.Pending, status);
    }

    [Fact]
    public async Task UpdateUser_AsAdmin_UpdatesFields()
    {
        const string phone = "+79995550209";
        await fixture.SeedPendingUserAsync(phone);
        using var client = await CreateLoggedInAdminClient();
        var users = await client.GetFromJsonAsync<EmployeeInfoDto[]>(UsersRoute);
        var seeded = Array.Find(users!, user => user.Phone == phone)!;

        var response = await client.PutAsJsonAsync(
            $"{UsersRoute}/{seeded.Id}",
            new UserDataRequest("Пётр", "Петров", "Петрович", "Ведущий инженер", phone));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var refreshed = await client.GetFromJsonAsync<EmployeeInfoDto[]>(UsersRoute);
        var updated = Array.Find(refreshed!, user => user.Id == seeded.Id)!;
        Assert.Equal("Петров", updated.Surname);
        Assert.Equal("Пётр", updated.Name);
        Assert.Equal("Петрович", updated.Patronymic);
        Assert.Equal("Ведущий инженер", updated.Position);
    }

    [Fact]
    public async Task UpdateUser_DuplicatePhone_ReturnsConflict()
    {
        const string phone = "+79995550210";
        const string otherPhone = "+79995550211";
        await fixture.SeedPendingUserAsync(phone);
        await fixture.SeedPendingUserAsync(otherPhone);
        using var client = await CreateLoggedInAdminClient();
        var users = await client.GetFromJsonAsync<EmployeeInfoDto[]>(UsersRoute);
        var seeded = Array.Find(users!, user => user.Phone == phone)!;

        var response = await client.PutAsJsonAsync(
            $"{UsersRoute}/{seeded.Id}",
            new UserDataRequest("Иван", "Иванов", null, "Инженер", otherPhone));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_UnknownId_ReturnsNotFound()
    {
        using var client = await CreateLoggedInAdminClient();

        var response = await client.PutAsJsonAsync(
            $"{UsersRoute}/{Guid.NewGuid()}",
            new UserDataRequest("Иван", "Иванов", null, "Инженер", "+79995550212"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
