using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Pandora.Domain.Enums;
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

    [Fact]
    public async Task GetPasswords_RequestedIds_ReturnsOnlyMatchingPasswords()
    {
        var store = CreateStore(
            """
            {"success":true,"data":{"object":"list","data":[
              {"id":"item-1","name":"One","login":{"username":"u1","password":"p1"}},
              {"id":"item-2","name":"Two","login":{"username":"u2","password":"p2"}}
            ]}}
            """);

        var result = await store.GetPasswords(["item-1"], CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("p1", Assert.Single(result.Value!).Value);
    }

    [Fact]
    public async Task GetPasswords_ItemWithoutPassword_IsSkipped()
    {
        var store = CreateStore(
            """
            {"success":true,"data":{"object":"list","data":[
              {"id":"item-1","name":"One","login":{"username":"u1","password":null}},
              {"id":"item-2","name":"Two","login":{"username":"u2","password":"p2"}}
            ]}}
            """);

        var result = await store.GetPasswords(["item-1", "item-2"], CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.ContainsKey("item-1"));
        Assert.Equal("p2", result.Value["item-2"]);
    }

    [Fact]
    public async Task GetPasswords_ItemWithNullId_IsSkipped()
    {
        var store = CreateStore(
            """
            {"success":true,"data":{"object":"list","data":[
              {"id":null,"name":"Broken","login":{"username":"u1","password":"p1"}},
              {"id":"item-2","name":"Two","login":{"username":"u2","password":"p2"}}
            ]}}
            """);

        var result = await store.GetPasswords(["item-2"], CancellationToken.None);

        Assert.True(result.IsSuccess);

        var passwords = result.Value!;

        Assert.Single(passwords);
        Assert.Equal("p2", passwords["item-2"]);
    }

    [Fact]
    public async Task GetPasswords_DuplicateIds_KeepsFirstPassword()
    {
        var store = CreateStore(
            """
            {"success":true,"data":{"object":"list","data":[
              {"id":"item-1","name":"One","login":{"username":"u1","password":"first"}},
              {"id":"item-1","name":"One","login":{"username":"u1","password":"second"}}
            ]}}
            """);

        var result = await store.GetPasswords(["item-1"], CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("first", result.Value!["item-1"]);
    }

    [Fact]
    public async Task GetPasswords_WhenGatewayFails_ReturnsVaultwardenUnavailable()
    {
        var store = CreateStore("""{"success":false,"message":"boom"}""", HttpStatusCode.InternalServerError);

        var result = await store.GetPasswords(["item-1"], CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.VaultwardenUnavailable.ToString(), result.ErrorCode);
    }

    private static VaultwardenSecretStore CreateStore(string body, HttpStatusCode statusCode = HttpStatusCode.OK)
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
