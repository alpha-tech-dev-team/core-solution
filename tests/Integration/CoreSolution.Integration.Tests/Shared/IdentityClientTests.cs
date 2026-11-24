using System.Net;
using System.Text;
using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Infrastructure.Identity;
using CoreSolution.Shared.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CoreSolution.Integration.Tests.Shared;

public sealed class IdentityClientTests
{
    private const string ServiceApiKey = "test-service-api-key";

    [Fact]
    public async Task ActivateUser_WhenIdentityReturns429WithEmptyBody_ReturnsServiceTemporarilyUnavailable()
    {
        var handler = new StubIdentityHandler(HttpStatusCode.TooManyRequests, string.Empty);
        var client = CreateClient(handler);

        var error = await client.ActivateUser(1001, "+79991234567", CancellationToken.None);

        Assert.Equal(ErrorCodes.ServiceTemporarilyUnavailable, error);
    }

    [Fact]
    public async Task CheckUser_WhenIdentityReturns429WithInvalidJsonBody_ReturnsServiceTemporarilyUnavailable()
    {
        var handler = new StubIdentityHandler(HttpStatusCode.TooManyRequests, "not-a-json", "text/plain");
        var client = CreateClient(handler);

        var error = await client.CheckUser(1001, "Atlas", CancellationToken.None);

        Assert.Equal(ErrorCodes.ServiceTemporarilyUnavailable, error);
    }

    [Fact]
    public async Task ActivateUser_WhenIdentityReturnsErrorCode_ReturnsCodeFromBody()
    {
        var handler = new StubIdentityHandler(HttpStatusCode.Forbidden, "{\"code\":\"UserBlocked\"}");
        var client = CreateClient(handler);

        var error = await client.ActivateUser(1001, "+79991234567", CancellationToken.None);

        Assert.Equal(ErrorCodes.UserBlocked, error);
    }

    [Fact]
    public async Task BotRequests_AreSentWithServiceApiKeyHeader()
    {
        var handler = new StubIdentityHandler(HttpStatusCode.OK, string.Empty);
        var client = CreateClient(handler);

        await client.ActivateUser(1001, "+79991234567", CancellationToken.None);
        await client.CheckUser(1001, "Atlas", CancellationToken.None);

        Assert.NotNull(handler.LastRequest);
        Assert.True(handler.LastRequest!.Headers.TryGetValues(ClientApiRoutes.ServiceApiKeyHeader, out var values));
        Assert.Equal(ServiceApiKey, Assert.Single(values));
    }

    private static IdentityClient CreateClient(StubIdentityHandler handler)
    {
        var options = Options.Create(new IdentityOptions
        {
            BaseUrl = "http://localhost",
            ServiceApiKey = ServiceApiKey
        });

        return new IdentityClient(
            new HttpClient(handler) { BaseAddress = new Uri("http://localhost") },
            options,
            NullLogger<IdentityClient>.Instance);
    }

    private sealed class StubIdentityHandler(
        HttpStatusCode statusCode,
        string content,
        string mediaType = "application/json") : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;

            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, mediaType)
            };

            return Task.FromResult(response);
        }
    }
}
