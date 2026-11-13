using System.Net;
using System.Net.Http.Json;
using Identity.Api.Models;
using Identity.Application.Contracts.Admin;
using Identity.Application.Contracts.Client;
using CoreSolution.Shared.Enums;
using Identity.Application.Errors;
using Identity.Domain.Enums;
using Xunit;

namespace CoreSolution.Integration.Tests.IdentityTests;

[Collection(nameof(IntegrationCollection))]
public sealed class AdminEndpointsTests
{
    private const string LoginRoute = "/api/admin/login";
    private const string UsersRoute = "/api/admin/users";
    private const string RolesRoute = "/api/admin/roles";

    private readonly IntegrationFixture _fixture;

    public AdminEndpointsTests(IntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetUsers_WithoutAuth_ReturnsUnauthorizedWithErrorCode()
    {
        using var client = _fixture.CreateIdentityHttpClient();

        var response = await client.GetAsync(UsersRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.InvalidCredentials), error!.Code);
    }

    [Fact]
    public async Task GetUsers_BlockedAdminWithValidToken_ReturnsUnauthorized()
    {
        var username = $"admin_{Guid.NewGuid():N}";
        var adminId = await _fixture.SeedAdminAsync(username, "admin-password-123");
        using var adminClient = _fixture.CreateIdentityHttpClientWithCookies();
        var login = await adminClient.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, "admin-password-123"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var editor = await CreateLoggedInAdminClient();
        var block = await editor.PutAsJsonAsync(
            $"{UsersRoute}/{adminId}/status",
            new SetUserStatusRequest(UserStatus.Blocked));
        Assert.Equal(HttpStatusCode.OK, block.StatusCode);

        var response = await adminClient.GetAsync(UsersRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.UserBlocked), error!.Code);
    }

    [Fact]
    public async Task GetUsers_ArchivedAdminWithValidToken_ReturnsUnauthorized()
    {
        var username = $"admin_{Guid.NewGuid():N}";
        var adminId = await _fixture.SeedAdminAsync(username, "admin-password-123");
        using var adminClient = _fixture.CreateIdentityHttpClientWithCookies();
        var login = await adminClient.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, "admin-password-123"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var editor = await CreateLoggedInAdminClient();
        var archive = await editor.DeleteAsync($"{UsersRoute}/{adminId}");
        Assert.Equal(HttpStatusCode.OK, archive.StatusCode);

        var response = await adminClient.GetAsync(UsersRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.AccessDenied), error!.Code);
    }

    [Fact]
    public async Task GetUsers_AdminRoleRemovedWithValidToken_ReturnsForbidden()
    {
        var username = $"admin_{Guid.NewGuid():N}";
        var adminId = await _fixture.SeedAdminAsync(username, "admin-password-123");
        using var adminClient = _fixture.CreateIdentityHttpClientWithCookies();
        var login = await adminClient.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, "admin-password-123"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var editor = await CreateLoggedInAdminClient();
        var roles = await editor.PutAsJsonAsync(
            $"{UsersRoute}/{adminId}/roles",
            new SetUserRolesRequest([UserRole.Atlas]));
        Assert.Equal(HttpStatusCode.OK, roles.StatusCode);

        var response = await adminClient.GetAsync(UsersRoute);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.AccessDenied), error!.Code);
    }

    [Fact]
    public async Task Refresh_WithoutCookie_ReturnsUnauthorizedWithErrorCode()
    {
        using var client = _fixture.CreateIdentityHttpClient();

        var response = await client.PostAsync("/api/admin/refresh", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.InvalidRefreshToken), error!.Code);
    }

    [Fact]
    public async Task GetUsers_AsAdmin_ReturnsSeededUserWithRoles()
    {
        const string phone = "+79995550201";
        await _fixture.SeedPendingUserAsync(phone, "Pandora");
        using var client = await CreateLoggedInAdminClient();

        var users = await GetUsers(client);
        var seeded = Assert.Single(users, user => user.Phone == phone);
        Assert.Equal(nameof(UserStatus.Pending), seeded.Status);
        Assert.Contains("Pandora", seeded.Roles);
    }

    [Fact]
    public async Task GetUsers_Pagination_ReturnsRequestedPageAndTotal()
    {
        using var client = await CreateLoggedInAdminClient();

        var first = await client.GetFromJsonAsync<EmployeesPageDto>($"{UsersRoute}?page=1&pageSize=1");
        var second = await client.GetFromJsonAsync<EmployeesPageDto>($"{UsersRoute}?page=2&pageSize=1");

        Assert.Single(first!.Items);
        Assert.Single(second!.Items);
        Assert.True(first.Total >= 2);
        Assert.NotEqual(first.Items[0].Id, second.Items[0].Id);
    }

    [Fact]
    public async Task GetUsers_InvalidPageSize_ReturnsBadRequest()
    {
        using var client = await CreateLoggedInAdminClient();

        var response = await client.GetAsync($"{UsersRoute}?pageSize=1000");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.ValidationFailed), error!.Code);
    }

    [Fact]
    public async Task GetRoles_AsAdmin_ReturnsSeededRoles()
    {
        await _fixture.SeedPendingUserAsync("+79995550202", "Atlas");
        using var client = await CreateLoggedInAdminClient();

        var response = await client.GetAsync(RolesRoute);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var roles = await response.Content.ReadFromJsonAsync<string[]>();
        Assert.Contains("Atlas", roles!);
    }

    [Fact]
    public async Task SetRoles_UnknownRole_ReturnsBadRequest()
    {
        using var client = await CreateLoggedInAdminClient();

        var response = await client.PutAsJsonAsync(
            $"{UsersRoute}/{Guid.NewGuid()}/roles",
            new { roles = new[] { "NoSuchRole" } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.ValidationFailed), error!.Code);
    }

    [Fact]
    public async Task SetStatus_UnknownValue_ReturnsBadRequest()
    {
        using var client = await CreateLoggedInAdminClient();

        var response = await client.PutAsJsonAsync(
            $"{UsersRoute}/{Guid.NewGuid()}/status",
            new { status = "Zzz" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.ValidationFailed), error!.Code);
    }

    [Fact]
    public async Task SetRoles_AdminRoleWithoutCredentials_ReturnsBadRequest()
    {
        const string phone = "+79995550214";
        await _fixture.SeedPendingUserAsync(phone);
        using var client = await CreateLoggedInAdminClient();
        var users = await GetUsers(client);
        var seeded = Array.Find(users, user => user.Phone == phone)!;

        var response = await client.PutAsJsonAsync(
            $"{UsersRoute}/{seeded.Id}/roles",
            new SetUserRolesRequest([UserRole.AtlasAdmin]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.ValidationFailed), error!.Code);
    }

    [Fact]
    public async Task SetRoles_AdminRoleWithCredentials_SavesAdminLogin()
    {
        const string phone = "+79995550215";
        await _fixture.SeedPendingUserAsync(phone, nameof(UserRole.PandoraAdmin));
        using var client = await CreateLoggedInAdminClient();
        var users = await GetUsers(client);
        var seeded = Array.Find(users, user => user.Phone == phone)!;

        var response = await client.PutAsJsonAsync(
            $"{UsersRoute}/{seeded.Id}/roles",
            new SetUserRolesRequest([UserRole.PandoraAdmin], "pandora-admin", "admin-password-123"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var refreshed = await GetUsers(client);
        var updated = Array.Find(refreshed, user => user.Id == seeded.Id)!;
        Assert.Equal("pandora-admin", updated.AdminUsername);
        Assert.Contains(nameof(UserRole.PandoraAdmin), updated.Roles);
    }

    [Fact]
    public async Task SetStatus_BlockedUser_LosesAccess()
    {
        const string phone = "+79995550204";
        const long telegramUserId = 9104;
        await _fixture.SeedPendingUserAsync(phone);
        using var client = await CreateLoggedInAdminClient();
        var users = await GetUsers(client);
        var seeded = Array.Find(users!, user => user.Phone == phone)!;
        _ = await client.PostAsJsonAsync("/api/client/activate", new ActivateUserRequest(telegramUserId, phone));

        var response = await client.PutAsJsonAsync($"{UsersRoute}/{seeded.Id}/status", new SetUserStatusRequest(UserStatus.Blocked));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var (_, status) = await _fixture.GetUserStateAsync(phone);
        Assert.Equal(UserStatus.Blocked, status);

        var check = await client.PostAsJsonAsync("/api/client/check", new CheckUserRequest(telegramUserId, "Atlas"));
        Assert.NotEqual(HttpStatusCode.OK, check.StatusCode);
    }

    [Fact]
    public async Task SetRoles_ExistingRole_ReplacesUserRoles()
    {
        const string phone = "+79995550205";
        await _fixture.SeedPendingUserAsync(phone, "Pandora");
        using var client = await CreateLoggedInAdminClient();
        var users = await GetUsers(client);
        var seeded = Array.Find(users!, user => user.Phone == phone)!;

        var response = await client.PutAsJsonAsync($"{UsersRoute}/{seeded.Id}/roles", new SetUserRolesRequest([UserRole.Pandora]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var refreshed = await GetUsers(client);
        var updated = Array.Find(refreshed!, user => user.Phone == phone)!;
        Assert.Equal(["Pandora"], updated.Roles);
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
        var users = await GetUsers(client);
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
        await _fixture.SeedPendingUserAsync(phone);
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
        var error = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.ValidationFailed), error!.Code);
        Assert.All(error.Errors.Values.SelectMany(messages => messages), message => Assert.DoesNotContain("Validator", message));
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
        await _fixture.SeedPendingUserAsync(phone);
        using var client = await CreateLoggedInAdminClient();
        var users = await GetUsers(client);
        var seeded = Array.Find(users!, user => user.Phone == phone)!;

        _ = await client.PutAsJsonAsync($"{UsersRoute}/{seeded.Id}/status", new SetUserStatusRequest(UserStatus.Blocked));
        var response = await client.PutAsJsonAsync($"{UsersRoute}/{seeded.Id}/status", new SetUserStatusRequest(UserStatus.Active));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var (_, status) = await _fixture.GetUserStateAsync(phone);
        Assert.Equal(UserStatus.Pending, status);
    }

    [Fact]
    public async Task DeleteUser_AsAdmin_RemovesUser()
    {
        const string phone = "+79995550216";
        await _fixture.SeedPendingUserAsync(phone);
        using var client = await CreateLoggedInAdminClient();
        var users = await GetUsers(client);
        var seeded = Array.Find(users, user => user.Phone == phone)!;

        var response = await client.DeleteAsync($"{UsersRoute}/{seeded.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var refreshed = await GetUsers(client);
        Assert.DoesNotContain(refreshed, user => user.Id == seeded.Id);
    }

    [Fact]
    public async Task DeleteUser_UnknownId_ReturnsNotFound()
    {
        using var client = await CreateLoggedInAdminClient();

        var response = await client.DeleteAsync($"{UsersRoute}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_AsAdmin_ArchivesAndKeepsRecord()
    {
        const string phone = "+79995550240";
        await _fixture.SeedPendingUserAsync(phone);
        using var client = await CreateLoggedInAdminClient();
        var users = await GetUsers(client);
        var seeded = Array.Find(users, user => user.Phone == phone)!;

        var response = await client.DeleteAsync($"{UsersRoute}/{seeded.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var (_, status) = await _fixture.GetUserStateAsync(phone);
        Assert.Equal(UserStatus.Archived, status);
        var refreshed = await GetUsers(client);
        Assert.DoesNotContain(refreshed, user => user.Id == seeded.Id);
    }

    [Fact]
    public async Task ArchivedUser_IsHiddenByDefaultAndRestorable()
    {
        const string phone = "+79995550243";
        await _fixture.SeedPendingUserAsync(phone);
        using var client = await CreateLoggedInAdminClient();
        var users = await GetUsers(client);
        var seeded = Array.Find(users, user => user.Phone == phone)!;
        var deletion = await client.DeleteAsync($"{UsersRoute}/{seeded.Id}");
        Assert.Equal(HttpStatusCode.OK, deletion.StatusCode);

        var defaultUsers = await GetUsers(client);
        Assert.DoesNotContain(defaultUsers, user => user.Id == seeded.Id);
        var archived = await client.GetFromJsonAsync<EmployeesPageDto>($"{UsersRoute}?includeArchived=true&pageSize=50");
        Assert.Contains(archived!.Items, user => user.Id == seeded.Id && user.Status == nameof(UserStatus.Archived));

        var restore = await client.PutAsJsonAsync(
            $"{UsersRoute}/{seeded.Id}/status",
            new SetUserStatusRequest(UserStatus.Active));

        Assert.Equal(HttpStatusCode.OK, restore.StatusCode);
        var restored = await GetUsers(client);
        Assert.Contains(restored, user => user.Id == seeded.Id);
    }

    [Fact]
    public async Task CreateUser_PhoneOfArchivedUser_ReturnsConflict()
    {
        const string phone = "+79995550241";
        await _fixture.SeedPendingUserAsync(phone);
        using var client = await CreateLoggedInAdminClient();
        var users = await GetUsers(client);
        var seeded = Array.Find(users, user => user.Phone == phone)!;
        var deletion = await client.DeleteAsync($"{UsersRoute}/{seeded.Id}");
        Assert.Equal(HttpStatusCode.OK, deletion.StatusCode);

        var response = await client.PostAsJsonAsync(
            UsersRoute,
            new UserDataRequest("Иван", "Иванов", null, "Инженер", phone));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.PhoneAlreadyExists), error!.Code);
    }

    [Fact]
    public async Task SetRoles_AdminLoginOfArchivedUser_ReturnsConflict()
    {
        const string phone = "+79995550242";
        var username = $"admin_{Guid.NewGuid():N}";
        var deletedAdminId = await _fixture.SeedAdminAsync(username, "admin-password-123");
        await _fixture.SeedPendingUserAsync(phone);
        using var client = await CreateLoggedInAdminClient();
        var deletion = await client.DeleteAsync($"{UsersRoute}/{deletedAdminId}");
        Assert.Equal(HttpStatusCode.OK, deletion.StatusCode);
        var users = await GetUsers(client);
        var target = Array.Find(users, user => user.Phone == phone)!;

        var response = await client.PutAsJsonAsync(
            $"{UsersRoute}/{target.Id}/roles",
            new SetUserRolesRequest([UserRole.PandoraAdmin], username, "admin-password-123"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.AdminLoginAlreadyExists), error!.Code);
    }

    [Fact]
    public async Task Login_ArchivedAdmin_ReturnsUnauthorized()
    {
        var username = $"admin_{Guid.NewGuid():N}";
        var deletedAdminId = await _fixture.SeedAdminAsync(username, "admin-password-123");
        using var client = await CreateLoggedInAdminClient();
        var deletion = await client.DeleteAsync($"{UsersRoute}/{deletedAdminId}");
        Assert.Equal(HttpStatusCode.OK, deletion.StatusCode);

        using var loginClient = _fixture.CreateIdentityHttpClientWithCookies();
        var response = await loginClient.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, "admin-password-123"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.InvalidCredentials), error!.Code);
    }

    [Fact]
    public async Task Login_BlockedAdmin_ReturnsForbiddenWithUserBlocked()
    {
        var username = $"admin_{Guid.NewGuid():N}";
        var blockedAdminId = await _fixture.SeedAdminAsync(username, "admin-password-123");
        using var client = await CreateLoggedInAdminClient();
        var block = await client.PutAsJsonAsync(
            $"{UsersRoute}/{blockedAdminId}/status",
            new SetUserStatusRequest(UserStatus.Blocked));
        Assert.Equal(HttpStatusCode.OK, block.StatusCode);

        using var loginClient = _fixture.CreateIdentityHttpClientWithCookies();
        var response = await loginClient.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, "admin-password-123"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.UserBlocked), error!.Code);
    }

    [Fact]
    public async Task UpdateUser_AsAdmin_UpdatesFields()
    {
        const string phone = "+79995550209";
        await _fixture.SeedPendingUserAsync(phone);
        using var client = await CreateLoggedInAdminClient();
        var users = await GetUsers(client);
        var seeded = Array.Find(users!, user => user.Phone == phone)!;

        var response = await client.PutAsJsonAsync(
            $"{UsersRoute}/{seeded.Id}",
            new UserDataRequest("Пётр", "Петров", "Петрович", "Ведущий инженер", phone));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var refreshed = await GetUsers(client);
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
        await _fixture.SeedPendingUserAsync(phone);
        await _fixture.SeedPendingUserAsync(otherPhone);
        using var client = await CreateLoggedInAdminClient();
        var users = await GetUsers(client);
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

    [Fact]
    public async Task SetRoles_DuplicateAdminLogin_ReturnsConflict()
    {
        const string firstPhone = "+79995550220";
        const string secondPhone = "+79995550221";
        await _fixture.SeedPendingUserAsync(firstPhone);
        await _fixture.SeedPendingUserAsync(secondPhone);
        using var client = await CreateLoggedInAdminClient();
        var users = await GetUsers(client);
        var first = Array.Find(users, user => user.Phone == firstPhone)!;
        var second = Array.Find(users, user => user.Phone == secondPhone)!;

        var firstResponse = await client.PutAsJsonAsync(
            $"{UsersRoute}/{first.Id}/roles",
            new SetUserRolesRequest([UserRole.AtlasAdmin], "duplicate-admin", "admin-password-123"));
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var secondResponse = await client.PutAsJsonAsync(
            $"{UsersRoute}/{second.Id}/roles",
            new SetUserRolesRequest([UserRole.AtlasAdmin], "duplicate-admin", "admin-password-123"));

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        var error = await secondResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.AdminLoginAlreadyExists), error!.Code);
    }

    [Fact]
    public async Task Login_AdminRoleRemoved_ReturnsForbidden()
    {
        var username = $"admin_{Guid.NewGuid():N}";
        await _fixture.SeedAdminAsync(username, "admin-password-123");

        using var client = await CreateLoggedInAdminClient();
        var users = await GetUsers(client);
        var target = Array.Find(users, user => user.AdminUsername == username)!;

        var setRoles = await client.PutAsJsonAsync(
            $"{UsersRoute}/{target.Id}/roles",
            new SetUserRolesRequest([UserRole.Atlas], null, null));
        Assert.Equal(HttpStatusCode.OK, setRoles.StatusCode);

        using var loginClient = _fixture.CreateIdentityHttpClientWithCookies();
        var response = await loginClient.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, "admin-password-123"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.AccessDenied), error!.Code);
    }

    [Fact]
    public async Task UpdateUser_SetsUpdatedByToActingAdmin()
    {
        var username = $"admin_{Guid.NewGuid():N}";
        var adminId = await _fixture.SeedAdminAsync(username, "admin-password-123");
        const string phone = "+79995550230";
        await _fixture.SeedPendingUserAsync(phone);
        using var client = _fixture.CreateIdentityHttpClientWithCookies();
        var login = await client.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, "admin-password-123"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var users = await GetUsers(client);
        var seeded = Array.Find(users, user => user.Phone == phone)!;

        var response = await client.PutAsJsonAsync(
            $"{UsersRoute}/{seeded.Id}",
            new UserDataRequest("Иван", "Петров", null, "Инженер", phone));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(adminId, await _fixture.GetUserUpdatedByAsync(phone));
    }

    [Fact]
    public async Task SetRoles_ShortAdminPassword_ReturnsBadRequest()
    {
        const string phone = "+79995550231";
        await _fixture.SeedPendingUserAsync(phone);
        using var client = await CreateLoggedInAdminClient();
        var users = await GetUsers(client);
        var seeded = Array.Find(users, user => user.Phone == phone)!;

        var response = await client.PutAsJsonAsync(
            $"{UsersRoute}/{seeded.Id}/roles",
            new SetUserRolesRequest([UserRole.AtlasAdmin], "admin-x", "short"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.ValidationFailed), error!.Code);
    }

    [Fact]
    public async Task GetUsers_AtlasAdminOnly_ReturnsForbidden()
    {
        var username = $"admin_{Guid.NewGuid():N}";
        await _fixture.SeedAdminAsync(username, "admin-password-123", nameof(UserRole.AtlasAdmin));
        using var client = _fixture.CreateIdentityHttpClientWithCookies();
        var login = await client.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, "admin-password-123"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var response = await client.GetAsync(UsersRoute);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SetRoles_RemoveOwnPandoraAdminRole_ReturnsForbidden()
    {
        var username = $"admin_{Guid.NewGuid():N}";
        await _fixture.SeedAdminAsync(username, "admin-password-123");
        using var client = _fixture.CreateIdentityHttpClientWithCookies();
        var login = await client.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, "admin-password-123"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var users = await GetUsers(client);
        var self = Array.Find(users, user => user.AdminUsername == username)!;

        var response = await client.PutAsJsonAsync(
            $"{UsersRoute}/{self.Id}/roles",
            new SetUserRolesRequest([UserRole.Atlas], null, null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.SelfActionForbidden), error!.Code);

        using var loginClient = _fixture.CreateIdentityHttpClientWithCookies();
        var retry = await loginClient.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, "admin-password-123"));
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
    }

    [Fact]
    public async Task SetStatus_BlockSelf_ReturnsForbidden()
    {
        var username = $"admin_{Guid.NewGuid():N}";
        await _fixture.SeedAdminAsync(username, "admin-password-123");
        using var client = _fixture.CreateIdentityHttpClientWithCookies();
        var login = await client.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, "admin-password-123"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var users = await GetUsers(client);
        var self = Array.Find(users, user => user.AdminUsername == username)!;

        var response = await client.PutAsJsonAsync(
            $"{UsersRoute}/{self.Id}/status",
            new SetUserStatusRequest(UserStatus.Blocked));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.SelfActionForbidden), error!.Code);

        using var loginClient = _fixture.CreateIdentityHttpClientWithCookies();
        var retry = await loginClient.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, "admin-password-123"));
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_Self_ReturnsForbidden()
    {
        var username = $"admin_{Guid.NewGuid():N}";
        await _fixture.SeedAdminAsync(username, "admin-password-123");
        using var client = _fixture.CreateIdentityHttpClientWithCookies();
        var login = await client.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, "admin-password-123"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var users = await GetUsers(client);
        var self = Array.Find(users, user => user.AdminUsername == username)!;

        var response = await client.DeleteAsync($"{UsersRoute}/{self.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(nameof(IdentityErrorCode.SelfActionForbidden), error!.Code);

        using var loginClient = _fixture.CreateIdentityHttpClientWithCookies();
        var retry = await loginClient.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, "admin-password-123"));
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
    }

    private async Task<HttpClient> CreateLoggedInAdminClient()
    {
        var username = $"admin_{Guid.NewGuid():N}";
        await _fixture.SeedAdminAsync(username, "admin-password-123");

        var client = _fixture.CreateIdentityHttpClientWithCookies();
        var response = await client.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, "admin-password-123"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return client;
    }

    private static async Task<EmployeeInfoDto[]> GetUsers(HttpClient client)
    {
        const int pageSize = 50;
        var first = await client.GetFromJsonAsync<EmployeesPageDto>($"{UsersRoute}?pageSize={pageSize}");
        var items = new List<EmployeeInfoDto>(first!.Items);
        var totalPages = (int)Math.Ceiling(first.Total / (double)pageSize);

        for (var page = 2; page <= totalPages; page++)
        {
            var next = await client.GetFromJsonAsync<EmployeesPageDto>($"{UsersRoute}?page={page}&pageSize={pageSize}");
            items.AddRange(next!.Items);
        }

        return items.ToArray();
    }

    private sealed record EmployeesPageDto(EmployeeInfoDto[] Items, int Total);

    private sealed record EmployeeInfoDto(
        Guid Id,
        string Name,
        string Surname,
        string? Patronymic,
        string Position,
        string Phone,
        string Status,
        string[] Roles,
        string? AdminUsername);
}
