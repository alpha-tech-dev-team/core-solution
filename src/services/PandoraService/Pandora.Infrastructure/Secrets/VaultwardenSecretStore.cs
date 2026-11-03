using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Infrastructure.Secrets;

public sealed class VaultwardenSecretStore : ISecretStore
{
    private const string ListRoute = "list/object/items";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<VaultwardenSecretStore> _logger;

    public VaultwardenSecretStore(HttpClient httpClient, ILogger<VaultwardenSecretStore> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Secret?> GetSecret(string itemId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"object/item/{itemId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Шлюз Vaultwarden вернул статус {StatusCode} для элемента {ItemId}", (int)response.StatusCode, itemId);

            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<VaultwardenResponse<VaultwardenItem>>(SerializerOptions, cancellationToken);
        var itemDetails = result?.Data;

        if (string.IsNullOrWhiteSpace(itemDetails?.Login?.Password))
        {
            _logger.LogWarning("В элементе {ItemId} нет пароля", itemId);

            return null;
        }

        return new Secret(itemDetails.Name, itemDetails.Login.Username ?? string.Empty, itemDetails.Login.Password);
    }

    public async Task<IReadOnlyList<SecretItem>> GetItems(CancellationToken cancellationToken)
    {
        var items = await FetchItems(cancellationToken);

        return items is null
            ? Array.Empty<SecretItem>()
            : items.Select(item => new SecretItem(item.Id, item.Name)).ToList();
    }

    private async Task<IReadOnlyList<VaultwardenItem>?> FetchItems(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(ListRoute, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Шлюз Vaultwarden вернул статус {StatusCode} при запросе списка", (int)response.StatusCode);

                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<VaultwardenResponse<VaultwardenList>>(SerializerOptions, cancellationToken);

            return result?.Data?.Data;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Не удалось получить список секретов из Vaultwarden");

            return null;
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "Не удалось разобрать ответ шлюза Vaultwarden");

            return null;
        }
    }

    private sealed record VaultwardenResponse<T>(bool Success, T? Data);
    private sealed record VaultwardenList(string? Object, IReadOnlyList<VaultwardenItem>? Data);
    private sealed record VaultwardenItem(string Id, string Name, VaultwardenLogin? Login);
    private sealed record VaultwardenLogin(string? Username, string? Password);
}
