using System.Net;
using System.Net.Http.Json;
using CoreSolution.Shared.Enums;
using Identity.Application.Contracts.Admin;
using Microsoft.AspNetCore.Mvc.Testing;
using Pandora.Domain.Entities;
using Pandora.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Domain.Constants;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

[Collection(nameof(IntegrationCollection))]
public sealed class AuditEndpointTests(IntegrationFixture fixture)
{
    private const string AuditRoute = "/api/admin/audit";
    private const string LoginRoute = "/api/admin/login";
    private const string AdminPassword = "admin-password-123";
    private const string TestSecret = "integration-test-secret-key-at-least-32-chars";
    private const string AccessTokenCookie = "access_token";

    [Fact]
    public async Task GetAudit_WithoutToken_ReturnsUnauthorized()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(AuditRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAudit_AsAtlasAdmin_ReturnsForbidden()
    {
        var cookie = await LoginAsAdminAsync(nameof(UserRole.AtlasAdmin));
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", cookie);

        var response = await client.GetAsync(AuditRoute);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAudit_WithInvalidPageSize_ReturnsBadRequest()
    {
        var cookie = await LoginAsAdminAsync();
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", cookie);

        var response = await client.GetAsync($"{AuditRoute}?page=0&pageSize=0");

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True("InvalidPage" == await ReadValidationCodeAsync(response), body);
    }

    [Fact]
    public async Task GetAudit_WithUnknownSortField_ReturnsBadRequest()
    {
        var cookie = await LoginAsAdminAsync();
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", cookie);

        var response = await client.GetAsync($"{AuditRoute}?sortBy=unknown");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("InvalidSortField", await ReadValidationCodeAsync(response));
    }

    [Fact]
    public async Task GetAudit_WithUnknownSortDirection_ReturnsBadRequest()
    {
        var cookie = await LoginAsAdminAsync();
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", cookie);

        var response = await client.GetAsync($"{AuditRoute}?sortDirection=999");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("InvalidSortDirection", await ReadValidationCodeAsync(response));
    }

    [Fact]
    public async Task GetAudit_WithTooLongSearch_ReturnsBadRequest()
    {
        var cookie = await LoginAsAdminAsync();
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", cookie);

        var search = new string('a', AuditConstants.SearchMaxLength + 1);

        var response = await client.GetAsync($"{AuditRoute}?search={search}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("InvalidSearch", await ReadValidationCodeAsync(response));
    }

    [Fact]
    public async Task GetAudit_WithSearchAndFilters_ReturnsMatchingEntries()
    {
        var targetId = Guid.NewGuid().ToString();
        await SeedAuditEntryAsync("secret_created", targetId, "web");
        await SeedAuditEntryAsync("secret_deleted", Guid.NewGuid().ToString(), "web");

        var cookie = await LoginAsAdminAsync();
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", cookie);

        var response = await client.GetAsync(
            $"{AuditRoute}?page=1&pageSize=10&search={targetId}&action=secret_created&requestSource=web&sortBy=action&sortDirection=Ascending");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<AuditPageResponse>();

        Assert.NotNull(page);
        var entry = Assert.Single(page!.Items);
        Assert.Equal("secret_created", entry.Action);
        Assert.Equal(targetId, entry.TargetItemId);
        Assert.Equal("web", entry.RequestSource);
    }

    [Fact]
    public async Task GetAudit_WithActorUserIds_ReturnsEntriesOfMatchedActors()
    {
        var userId = Guid.NewGuid();
        await SeedAuditEntryAsync(userId: userId);

        var cookie = await LoginAsAdminAsync();
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", cookie);

        var response = await client.GetAsync($"{AuditRoute}?page=1&pageSize=10&search=иван&userIds={userId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<AuditPageResponse>();

        Assert.NotNull(page);
        var entry = Assert.Single(page!.Items);
        Assert.Equal(userId, entry.UserId);
    }

    [Fact]
    public async Task GetAudit_AsAdmin_ReturnsStoredEntry()
    {
        await SeedAuditEntryAsync();
        var cookie = await LoginAsAdminAsync();
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", cookie);

        var response = await client.GetAsync($"{AuditRoute}?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await response.Content.ReadFromJsonAsync<AuditPageResponse>();

        Assert.NotNull(page);
        Assert.True(page!.Total > 0);
        Assert.Contains(page.Items, entry => entry.Action == "secret_created" && entry.RequestSource == "web");
    }

    private async Task SeedAuditEntryAsync(
        string action = "secret_created",
        string? targetItemId = null,
        string requestSource = "web",
        Guid? userId = null)
    {
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        database.AuditEntry.Add(new AuditEntry(
            Guid.NewGuid(),
            DateTime.UtcNow,
            userId ?? Guid.NewGuid(),
            action,
            targetItemId,
            requestSource));

        await database.SaveChangesAsync();
    }

    private static async Task<string?> ReadValidationCodeAsync(HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        return problem is not null && problem.TryGetValue("code", out var code) ? code.ToString() : null;
    }

    private async Task<string> LoginAsAdminAsync(string roleName = nameof(UserRole.PandoraAdmin))
    {
        var username = $"admin_{Guid.NewGuid():N}";
        await fixture.SeedAdminAsync(username, AdminPassword, roleName);

        using var client = fixture.CreateIdentityHttpClient();
        var response = await client.PostAsJsonAsync(LoginRoute, new AdminLoginRequest(username, AdminPassword));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var token = ReadCookie(response, AccessTokenCookie);

        return $"{AccessTokenCookie}={token}";
    }

    private static string ReadCookie(HttpResponseMessage response, string name)
    {
        var header = Assert.Single(
            response.Headers.GetValues("Set-Cookie"),
            value => value.StartsWith($"{name}=", StringComparison.Ordinal));

        return header.Split(';')[0][(name.Length + 1)..];
    }

    private WebApplicationFactory<Pandora.Api.AssemblyMarker> CreateFactory()
    {
        return new WebApplicationFactory<Pandora.Api.AssemblyMarker>()
            .WithWebHostBuilder(builder => builder
                .UseSetting("Database:ConnectionString", fixture.PandoraConnectionString)
                .UseSetting("Jwt:Secret", TestSecret)
                .UseSetting("Jwt:Issuer", "integration-tests")
                .UseSetting("Jwt:Audience", "integration-tests"));
    }

    private sealed record AuditPageResponse(AuditEntryResponse[] Items, int Page, int PageSize, int Total);

    private sealed record AuditEntryResponse(
        Guid Id,
        DateTime Timestamp,
        Guid? UserId,
        long? TelegramUserId,
        string Action,
        string? TargetItemId,
        string RequestSource);
}
