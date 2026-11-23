using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Pandora.Domain.Enums;
using Pandora.Infrastructure.Secrets;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

public sealed class VaultwardenSecretStoreTests
{
    private const string ItemId = "item-1";

    private static string ItemWithPassword(string itemId, string password)
    {
        return ItemBody(itemId, password);
    }

    private static string ItemWithoutPassword(string itemId)
    {
        return ItemBody(itemId, null);
    }

    private static string ItemBody(string itemId, string? password)
    {
        return JsonSerializer.Serialize(new
        {
            success = true,
            data = new
            {
                id = itemId,
                name = "One",
                login = new { username = "u1", password }
            }
        });
    }

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
        var store = CreateItemStore(new Dictionary<string, string>
        {
            [ItemId] = ItemWithPassword(ItemId, "p1"),
            ["item-2"] = ItemWithPassword("item-2", "p2")
        });

        var result = await store.GetPasswords([ItemId], CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("p1", Assert.Single(result.Value!).Value);
    }

    [Fact]
    public async Task GetPasswords_ItemWithoutPassword_IsSkipped()
    {
        var store = CreateItemStore(new Dictionary<string, string>
        {
            [ItemId] = ItemWithoutPassword(ItemId),
            ["item-2"] = ItemWithPassword("item-2", "p2")
        });

        var result = await store.GetPasswords([ItemId, "item-2"], CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.ContainsKey(ItemId));
        Assert.Equal("p2", result.Value["item-2"]);
    }

    [Fact]
    public async Task GetPasswords_MissingItem_IsSkipped()
    {
        var store = CreateItemStore(
            new Dictionary<string, string>
            {
                ["item-2"] = ItemWithPassword("item-2", "p2")
            },
            missingStatusCode: HttpStatusCode.BadRequest);

        var result = await store.GetPasswords([ItemId, "item-2"], CancellationToken.None);

        Assert.True(result.IsSuccess);

        var passwords = result.Value!;

        Assert.Single(passwords);
        Assert.Equal("p2", passwords["item-2"]);
    }

    [Fact]
    public async Task GetPasswords_DuplicateIds_RequestsEachItemOnce()
    {
        var handler = new RoutingHandler(
            new Dictionary<string, string>
            {
                [ItemId] = ItemWithPassword(ItemId, "p1")
            },
            HttpStatusCode.BadRequest);

        var store = CreateStore(handler);

        var result = await store.GetPasswords([ItemId, ItemId], CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("p1", result.Value![ItemId]);
        Assert.Equal(1, handler.RequestCount(ItemId));
    }

    [Fact]
    public async Task GetPasswords_WhenGatewayFails_ReturnsVaultwardenUnavailable()
    {
        var store = CreateStore("""{"success":false,"message":"boom"}""", HttpStatusCode.InternalServerError);

        var result = await store.GetPasswords(["item-1"], CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.VaultwardenUnavailable.ToString(), result.ErrorCode);
    }

    [Fact]
    public async Task GetPasswords_WhenGatewayReturnsBrokenBody_ReturnsVaultwardenUnavailable()
    {
        var store = CreateStore("""not-json""");

        var result = await store.GetPasswords([ItemId], CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.VaultwardenUnavailable.ToString(), result.ErrorCode);
    }

    [Fact]
    public async Task GetPasswords_WhenVaultIsLocked_ReturnsVaultwardenUnavailable()
    {
        var store = CreateStore("""{"success":false,"message":"Vault is locked."}""", HttpStatusCode.BadRequest);

        var result = await store.GetPasswords([ItemId], CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.VaultwardenUnavailable.ToString(), result.ErrorCode);
    }

    [Fact]
    public async Task GetSecret_WhenVaultIsLocked_ReturnsVaultwardenUnavailable()
    {
        var store = CreateStore("""{"success":false,"message":"Vault is locked."}""", HttpStatusCode.BadRequest);

        var result = await store.GetSecret(ItemId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.VaultwardenUnavailable.ToString(), result.ErrorCode);
    }

    [Fact]
    public async Task GetPasswords_MoreIdsThanParallelism_ReturnsEveryPassword()
    {
        var bodies = Enumerable.Range(1, 12)
            .ToDictionary(
                index => $"item-{index}",
                index => ItemWithPassword($"item-{index}", $"p{index}"),
                StringComparer.Ordinal);

        var store = CreateItemStore(bodies);

        var result = await store.GetPasswords(bodies.Keys.ToList(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Value!.Count);

        foreach (var itemId in bodies.Keys)
        {
            Assert.Equal($"p{itemId[5..]}", result.Value[itemId]);
        }
    }

    [Fact]
    public async Task GetPasswords_WithEmptyIds_RequestsNothing()
    {
        var handler = new RoutingHandler(
            new Dictionary<string, string>
            {
                [ItemId] = ItemWithPassword(ItemId, "p1")
            },
            HttpStatusCode.BadRequest);

        var store = CreateStore(handler);

        var result = await store.GetPasswords([string.Empty, "   "], CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
        Assert.Equal(0, handler.TotalRequests);
    }

    [Fact]
    public async Task GetPasswords_NeverExceedsParallelismLimit()
    {
        var handler = new ConcurrencyHandler();
        var store = CreateStore(handler);
        var itemIds = Enumerable.Range(1, 12).Select(index => $"item-{index}").ToList();

        var call = store.GetPasswords(itemIds, CancellationToken.None);

        var waited = 0;

        while (handler.MaxObserved < 4 && !call.IsCompleted && waited < 500)
        {
            await Task.Delay(10);

            waited++;
        }

        var observed = handler.MaxObserved;

        handler.Release();

        var result = await call;

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Value!.Count);
        Assert.InRange(observed, 1, 4);
    }

    private static VaultwardenSecretStore CreateStore(string body, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return CreateStore(new StubHandler(body, statusCode));
    }

    private static VaultwardenSecretStore CreateItemStore(
        IReadOnlyDictionary<string, string> bodiesByItemId,
        HttpStatusCode missingStatusCode = HttpStatusCode.OK)
    {
        return CreateStore(new RoutingHandler(bodiesByItemId, missingStatusCode));
    }

    private static VaultwardenSecretStore CreateStore(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler)
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

    private sealed class ConcurrencyHandler : HttpMessageHandler
    {
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        private int _current;

        public int MaxObserved { get; private set; }

        public void Release()
        {
            _release.TrySetResult();
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var current = Interlocked.Increment(ref _current);

            if (current > MaxObserved)
            {
                MaxObserved = current;
            }

            await _release.Task.WaitAsync(cancellationToken);

            Interlocked.Decrement(ref _current);

            var itemId = request.RequestUri?.Segments[^1] ?? string.Empty;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ItemWithPassword(itemId, "p1"), Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed class RoutingHandler(
        IReadOnlyDictionary<string, string> bodiesByItemId,
        HttpStatusCode missingStatusCode) : HttpMessageHandler
    {
        private readonly ConcurrentDictionary<string, int> _requests = new(StringComparer.Ordinal);

        public int RequestCount(string itemId)
        {
            return _requests.TryGetValue(itemId, out var count) ? count : 0;
        }

        public int TotalRequests
        {
            get
            {
                return _requests.Values.Sum();
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var itemId = request.RequestUri?.Segments[^1] ?? string.Empty;

            _requests.AddOrUpdate(itemId, 1, (_, count) => count + 1);

            var response = bodiesByItemId.TryGetValue(itemId, out var body)
                ? new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                }
                : new HttpResponseMessage(missingStatusCode)
                {
                    Content = new StringContent(
                        """{"success":false,"message":"Not found."}""",
                        Encoding.UTF8,
                        "application/json")
                };

            return Task.FromResult(response);
        }
    }
}
