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

        var response = await _httpClient.PostAsJsonAsync(ItemRoute, item, SerializerOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Шлюз Vaultwarden вернул статус {StatusCode} при создании секрета {Name}", (int)response.StatusCode, name);

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
        var response = await _httpClient.PutAsJsonAsync($"{ItemRoute}/{itemId}", item, SerializerOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Шлюз Vaultwarden вернул статус {StatusCode} при обновлении секрета {ItemId}", (int)response.StatusCode, itemId);

            return false;
        }

        return true;
    }

    public async Task<bool> Delete(string itemId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.DeleteAsync($"{ItemRoute}/{itemId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Шлюз Vaultwarden вернул статус {StatusCode} при удалении секрета {ItemId}", (int)response.StatusCode, itemId);

            return false;
        }

        return true;
    }

    private async Task<VaultwardenItem?> ReadItem(string itemId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"{ItemRoute}/{itemId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Шлюз Vaultwarden вернул статус {StatusCode} при чтении секрета {ItemId}", (int)response.StatusCode, itemId);

            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<VaultwardenResponse<VaultwardenItem>>(SerializerOptions, cancellationToken);

        return result?.Data;
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
