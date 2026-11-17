using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Infrastructure.Secrets;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

public sealed class VaultwardenSecretStoreTests
{
    private const string ItemId = "item-1";

    [Fact]
    public async Task GetSecret_WithNameAndPassword_ReturnsSecret()
    {
        var store = CreateStore("""{"success":true,"data":{"id":"item-1","name":"ClickUp","login":{"username":"user","password":"pass"}}}""");

        var result = await store.GetSecret(ItemId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ClickUp", result.Value!.ItemName);
        Assert.Equal("user", result.Value.Login);
        Assert.Equal("pass", result.Value.Password);
    }

    [Fact]
    public async Task GetSecret_WithoutName_ReturnsSecretNotFound()
    {
        var store = CreateStore("""{"success":true,"data":{"id":"item-1","name":null,"login":{"username":"user","password":"pass"}}}""");

        var result = await store.GetSecret(ItemId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.SecretNotFound.ToString(), result.ErrorCode);
    }

    [Fact]
    public async Task GetSecret_WithBlankName_ReturnsSecretNotFound()
    {
        var store = CreateStore("""{"success":true,"data":{"id":"item-1","name":"   ","login":{"username":"user","password":"pass"}}}""");

        var result = await store.GetSecret(ItemId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.SecretNotFound.ToString(), result.ErrorCode);
    }

    [Fact]
    public async Task GetSecret_WithoutPassword_ReturnsSecretNotFound()
    {
        var store = CreateStore("""{"success":true,"data":{"id":"item-1","name":"ClickUp","login":{"username":"user","password":null}}}""");

        var result = await store.GetSecret(ItemId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.SecretNotFound.ToString(), result.ErrorCode);
    }

    [Fact]
    public async Task GetSecret_WithMissingItem_ReturnsSecretNotFound()
    {
        var store = CreateStore("""{"success":false,"message":"Not found."}""", HttpStatusCode.BadRequest);

        var result = await store.GetSecret(ItemId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.SecretNotFound.ToString(), result.ErrorCode);
    }

    [Fact]
    public async Task GetSecret_WhenGatewayReturnsServerError_ReturnsVaultwardenUnavailable()
    {
        var store = CreateStore("""{"success":false,"message":"boom"}""", HttpStatusCode.InternalServerError);

        var result = await store.GetSecret(ItemId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.VaultwardenUnavailable.ToString(), result.ErrorCode);
    }

    private static ISecretStore CreateStore(string body, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var client = new HttpClient(new StubHandler(body, statusCode))
        {
            BaseAddress = new Uri("http://localhost")
        };

        return new VaultwardenSecretStore(client, NullLogger<VaultwardenSecretStore>.Instance);
    }

    private sealed class StubHandler(string body, HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}
