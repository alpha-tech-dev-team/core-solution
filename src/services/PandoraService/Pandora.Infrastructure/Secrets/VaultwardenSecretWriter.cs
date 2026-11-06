using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Infrastructure.Secrets;

public sealed class VaultwardenSecretWriter : ISecretWriter
{
    private const string ItemRoute = "object/item";
    private const int LoginItemType = 1;
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<VaultwardenSecretWriter> _logger;

    public VaultwardenSecretWriter(HttpClient httpClient, ILogger<VaultwardenSecretWriter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SecretItem?> Create(string name, string username, string password, CancellationToken cancellationToken)
    {
        var item = BuildItem(null, name, username, password);
        using var response = await SendAsync(HttpMethod.Post, ItemRoute, item, "создании", name, cancellationToken);

        if (response is null)
        {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<VaultwardenResponse<VaultwardenItem>>(SerializerOptions, cancellationToken);
        var created = result?.Data;

        if (created is null || string.IsNullOrWhiteSpace(created.Id))
        {
            _logger.LogWarning("Шлюз Vaultwarden не вернул id созданного секрета {Name}", name);

            return null;
        }

        return new SecretItem(created.Id, created.Name ?? name);
    }

    public async Task<bool> Update(string itemId, string name, string username, string? password, CancellationToken cancellationToken)
    {
        var updatedPassword = password;

        if (updatedPassword is null)
        {
            var current = await ReadItem(itemId, cancellationToken);

            if (current is null)
            {
                return false;
            }

            updatedPassword = current.Login?.Password;

            if (string.IsNullOrWhiteSpace(updatedPassword))
            {
                _logger.LogWarning("В элементе {ItemId} нет пароля, новый пароль не передан", itemId);

                return false;
            }
        }

        var item = BuildItem(itemId, name, username, updatedPassword);
        using var response = await SendAsync(HttpMethod.Put, $"{ItemRoute}/{itemId}", item, "обновлении", itemId, cancellationToken);

        return response is not null;
    }

    public async Task<bool> Delete(string itemId, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Delete, $"{ItemRoute}/{itemId}", null, "удалении", itemId, cancellationToken);

        return response is not null;
    }

    private async Task<VaultwardenItem?> ReadItem(string itemId, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Get, $"{ItemRoute}/{itemId}", null, "чтении", itemId, cancellationToken);

        if (response is null)
        {
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<VaultwardenResponse<VaultwardenItem>>(SerializerOptions, cancellationToken);

        return result?.Data;
    }

    private async Task<HttpResponseMessage?> SendAsync(
        HttpMethod method,
        string route,
        object? payload,
        string operation,
        string target,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, route);

        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload, options: SerializerOptions);
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Шлюз Vaultwarden вернул статус {StatusCode} при {Operation} секрета {Target}", (int)response.StatusCode, operation, target);

            response.Dispose();

            return null;
        }

        return response;
    }

    private static VaultwardenItemRequest BuildItem(string? itemId, string name, string username, string password)
    {
        return new VaultwardenItemRequest
        {
            Id = itemId,
            OrganizationId = null,
            Type = LoginItemType,
            Name = name,
            Notes = null,
            Favorite = false,
            Reprompt = 0,
            Login = new VaultwardenLoginRequest
            {
                Username = username,
                Password = password,
                SecurityAnswers = new(),
                Uris = new()
            },
            CollectionIds = new()
        };
    }

    private sealed record VaultwardenResponse<T>(T? Data);
    private sealed record VaultwardenItem(string? Id, string? Name, VaultwardenLogin? Login);
    private sealed record VaultwardenLogin(string? Username, string? Password);

    private sealed class VaultwardenItemRequest
    {
        public string? Id { get; init; }
        public string? OrganizationId { get; init; }
        public int Type { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Notes { get; init; }
        public bool Favorite { get; init; }
        public int Reprompt { get; init; }
        public VaultwardenLoginRequest? Login { get; init; }
        public List<string> CollectionIds { get; init; } = new();
    }

    private sealed class VaultwardenLoginRequest
    {
        public string? Username { get; init; }
        public string? Password { get; init; }
        public List<string> SecurityAnswers { get; init; } = new();
        public List<string> Uris { get; init; } = new();
    }
}
