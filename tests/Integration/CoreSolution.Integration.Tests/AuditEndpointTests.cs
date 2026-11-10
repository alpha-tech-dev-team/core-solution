using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using Xunit;

namespace CoreSolution.Integration.Tests;

[Collection(nameof(IntegrationCollection))]
public sealed class AuditEndpointTests(IntegrationFixture fixture)
{
    private const string AuditRoute = "/api/admin/audit";

    [Fact]
    public async Task GetAudit_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateApiClient();

        var response = await client.GetAsync(AuditRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private HttpClient CreateApiClient()
    {
        return new WebApplicationFactory<Pandora.Api.AssemblyMarker>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:ConnectionString"] = fixture.PandoraConnectionString,
                    ["Jwt:Secret"] = "integration-test-secret-key-at-least-32-chars",
                    ["Jwt:Issuer"] = "integration-tests",
                    ["Jwt:Audience"] = "integration-tests"
                });
            });
        }).CreateClient();
    }
}
