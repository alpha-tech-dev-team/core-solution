using System.Net;
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
    private const int DefaultReprompt = 0;
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
        var (response, _) = await SendAsync(HttpMethod.Post, ItemRoute, item, "создании", name, cancellationToken);

        if (response is null)
        {
            return null;
        }

        using (response)
        {
            var result = await response.Content.ReadFromJsonAsync<VaultwardenResponse<VaultwardenItem>>(SerializerOptions, cancellationToken);
            var created = result?.Data;

            if (created is null || string.IsNullOrWhiteSpace(created.Id))
            {
                _logger.LogWarning("Шлюз Vaultwarden не вернул id созданного секрета {Name}", name);

                return null;
            }

            return new SecretItem(created.Id, created.Name ?? name);
        }
    }

    public async Task<SecretUpdateResult> Update(string itemId, SecretUpdateData data, CancellationToken cancellationToken)
    {
        var (current, readStatus) = await ReadItem(itemId, cancellationToken);

        if (readStatus is GatewayStatus.NotFound || current?.DeletedDate is not null)
        {
            return SecretUpdateResult.NotFound;
        }

        if (current is null)
        {
            return SecretUpdateResult.GatewayError;
        }

        var updatedPassword = data.Password;

        if (updatedPassword is null)
        {
            updatedPassword = current.Login?.Password;

            if (string.IsNullOrWhiteSpace(updatedPassword))
            {
                _logger.LogWarning("В элементе {ItemId} нет пароля, новый пароль не передан", itemId);

                return SecretUpdateResult.NoPassword;
            }
        }

        var item = BuildUpdateItem(current, data.Name, data.Username, updatedPassword);
        var (response, status) = await SendAsync(HttpMethod.Put, $"{ItemRoute}/{itemId}", item, "обновлении", itemId, cancellationToken);

        response?.Dispose();

        return status switch
        {
            GatewayStatus.Success => SecretUpdateResult.Success,
            GatewayStatus.NotFound => SecretUpdateResult.NotFound,
            _ => SecretUpdateResult.GatewayError
        };
    }

    public async Task<SecretDeleteResult> Delete(string itemId, CancellationToken cancellationToken)
    {
        var (response, status) = await SendAsync(HttpMethod.Delete, $"{ItemRoute}/{itemId}", null, "удалении", itemId, cancellationToken, treatBadRequestAsNotFound: true);

        response?.Dispose();

        return status switch
        {
            GatewayStatus.Success => SecretDeleteResult.Success,
            GatewayStatus.NotFound => SecretDeleteResult.NotFound,
            _ => SecretDeleteResult.GatewayError
        };
    }

    private async Task<(VaultwardenItem? Item, GatewayStatus Status)> ReadItem(string itemId, CancellationToken cancellationToken)
    {
        var (response, status) = await SendAsync(HttpMethod.Get, $"{ItemRoute}/{itemId}", null, "чтении", itemId, cancellationToken, treatBadRequestAsNotFound: true);

        if (response is null)
        {
            return (null, status);
        }

        using (response)
        {
            var result = await response.Content.ReadFromJsonAsync<VaultwardenResponse<VaultwardenItem>>(SerializerOptions, cancellationToken);

            return (result?.Data, GatewayStatus.Success);
        }
    }

    private async Task<(HttpResponseMessage? Response, GatewayStatus Status)> SendAsync(
        HttpMethod method,
        string route,
        object? payload,
        string operation,
        string target,
        CancellationToken cancellationToken,
        bool treatBadRequestAsNotFound = false)
    {
        using var request = new HttpRequestMessage(method, route);

        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload, options: SerializerOptions);
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return (response, GatewayStatus.Success);
        }

        _logger.LogWarning("Шлюз Vaultwarden вернул статус {StatusCode} при {Operation} секрета {Target}", (int)response.StatusCode, operation, target);

        var status = response.StatusCode switch
        {
            HttpStatusCode.NotFound => GatewayStatus.NotFound,
            HttpStatusCode.BadRequest when treatBadRequestAsNotFound => GatewayStatus.NotFound,
            _ => GatewayStatus.Failure
        };

        response.Dispose();

        return (null, status);
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
            Reprompt = DefaultReprompt,
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

    private static VaultwardenItemRequest BuildUpdateItem(VaultwardenItem current, string name, string username, string password)
    {
        return new VaultwardenItemRequest
        {
            Id = current.Id,
            OrganizationId = current.OrganizationId,
            Type = LoginItemType,
            Name = name,
            Notes = current.Notes,
            Favorite = current.Favorite,
            Reprompt = current.Reprompt,
            Login = new VaultwardenLoginRequest
            {
                Username = username,
                Password = password,
                SecurityAnswers = current.Login?.SecurityAnswers ?? new(),
                Uris = current.Login?.Uris ?? new()
            },
            CollectionIds = current.CollectionIds ?? new()
        };
    }

    private enum GatewayStatus
    {
        Success,
        NotFound,
        Failure
    }

    private sealed record VaultwardenResponse<T>(T? Data);

    private sealed record VaultwardenItem(
        string? Id,
        string? Name,
        string? OrganizationId,
        VaultwardenLogin? Login,
        List<string>? CollectionIds,
        bool Favorite,
        int Reprompt,
        string? Notes,
        DateTime? DeletedDate);

    private sealed record VaultwardenLogin(string? Username, string? Password, List<string>? SecurityAnswers, List<string>? Uris);

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
