using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.Infrastructure.Secrets.Vaultwarden;

namespace Pandora.Infrastructure.Secrets;

public sealed class VaultwardenSecretWriter : ISecretWriter
{
    private const string ItemRoute = "object/item";
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
        var item = VaultwardenItem.Empty.ToItemRequest(name, username, password);
        var (response, _) = await SendAsync(HttpMethod.Post, ItemRoute, item, name, cancellationToken);

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

        var item = current.ToItemRequest(data.Name, data.Username, updatedPassword);
        var (response, status) = await SendAsync(HttpMethod.Put, $"{ItemRoute}/{itemId}", item, itemId, cancellationToken);

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
        var (response, status) = await SendAsync(HttpMethod.Delete, $"{ItemRoute}/{itemId}", null, itemId, cancellationToken, treatBadRequestAsNotFound: true);

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
        var (response, status) = await SendAsync(HttpMethod.Get, $"{ItemRoute}/{itemId}", null, itemId, cancellationToken, treatBadRequestAsNotFound: true);

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

        _logger.LogWarning("Шлюз Vaultwarden вернул статус {StatusCode} на {Method} секрета {Target}", (int)response.StatusCode, method.Method, target);

        var status = response.StatusCode switch
        {
            HttpStatusCode.NotFound => GatewayStatus.NotFound,
            HttpStatusCode.BadRequest when treatBadRequestAsNotFound => GatewayStatus.NotFound,
            _ => GatewayStatus.Failure
        };

        response.Dispose();

        return (null, status);
    }
}
