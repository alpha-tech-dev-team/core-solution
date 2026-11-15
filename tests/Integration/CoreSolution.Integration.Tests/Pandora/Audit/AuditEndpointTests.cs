using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

[Collection(nameof(IntegrationCollection))]
public sealed class AuditEndpointTests(IntegrationFixture fixture)
{
    private const string AuditRoute = "/api/admin/audit";
    private const string TestSecret = "integration-test-secret-key-at-least-32-chars";

    [Fact]
    public async Task GetAudit_WithoutToken_ReturnsUnauthorized()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(AuditRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
}
