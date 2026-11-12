using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

[Collection(nameof(IntegrationCollection))]
public sealed class AuditEndpointTests(IntegrationFixture fixture)
{
    private const string AuditRoute = "/api/admin/audit";
    private const string ConnectionStringVariable = "Database__ConnectionString";
    private const string JwtSecretVariable = "Jwt__Secret";
    private const string JwtIssuerVariable = "Jwt__Issuer";
    private const string JwtAudienceVariable = "Jwt__Audience";
    private const string TestSecret = "integration-test-secret-key-at-least-32-chars";

    [Fact]
    public async Task GetAudit_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateApiClient();

        var response = await client.GetAsync(AuditRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private HttpClient CreateApiClient()
    {
        var values = new Dictionary<string, string>
        {
            [ConnectionStringVariable] = fixture.PandoraConnectionString,
            [JwtSecretVariable] = TestSecret,
            [JwtIssuerVariable] = "integration-tests",
            [JwtAudienceVariable] = "integration-tests"
        };

        var previous = values.Keys.ToDictionary(name => name, Environment.GetEnvironmentVariable);

        foreach (var (name, value) in values)
        {
            Environment.SetEnvironmentVariable(name, value);
        }

        try
        {
            return new WebApplicationFactory<Pandora.Api.AssemblyMarker>().CreateClient();
        }
        finally
        {
            foreach (var (name, value) in previous)
            {
                Environment.SetEnvironmentVariable(name, value);
            }
        }
    }
}