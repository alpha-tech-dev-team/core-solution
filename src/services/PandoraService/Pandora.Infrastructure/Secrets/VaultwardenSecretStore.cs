using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CoreSolution.Shared.Results;
using Microsoft.Extensions.Logging;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.Infrastructure.Secrets.Vaultwarden;

namespace Pandora.Infrastructure.Secrets;

public sealed class VaultwardenSecretStore : ISecretStore, ISecretVaultReader
{
    private const string ListRoute = "list/object/items";
    private const string ItemRoute = "object/item";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<VaultwardenSecretStore> _logger;

    public VaultwardenSecretStore(HttpClient httpClient, ILogger<VaultwardenSecretStore> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<Secret>> GetSecret(string itemId, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.GetAsync($"{ItemRoute}/{itemId}", cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Не удалось получить секрет {ItemId} из Vaultwarden", itemId);

            return Result<Secret>.Failure(ErrorCode.VaultwardenUnavailable.ToString());
        }

        VaultwardenResponse<VaultwardenItem>? result;

        try
        {
            result = await response.Content.ReadFromJsonAsync<VaultwardenResponse<VaultwardenItem>>(SerializerOptions, cancellationToken);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "Не удалось разобрать ответ шлюза Vaultwarden для элемента {ItemId}", itemId);

            return Result<Secret>.Failure(ErrorCode.VaultwardenUnavailable.ToString());
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Шлюз Vaultwarden вернул статус {StatusCode} для элемента {ItemId}", (int)response.StatusCode, itemId);

            return IsMissingItem(response, result)
                ? Result<Secret>.Failure(ErrorCode.SecretNotFound.ToString())
                : Result<Secret>.Failure(ErrorCode.VaultwardenUnavailable.ToString());
        }

        var itemDetails = result?.Data;

        if (string.IsNullOrWhiteSpace(itemDetails?.Name))
        {
            _logger.LogWarning("В элементе {ItemId} нет названия", itemId);

            return Result<Secret>.Failure(ErrorCode.SecretNotFound.ToString());
        }

        if (string.IsNullOrWhiteSpace(itemDetails.Login?.Password))
        {
            _logger.LogWarning("В элементе {ItemId} нет пароля", itemId);

            return Result<Secret>.Failure(ErrorCode.SecretNotFound.ToString());
        }

        return Result<Secret>.Success(new Secret(itemDetails.Name, itemDetails.Login.Username ?? string.Empty, itemDetails.Login.Password));
    }

    public async Task<Result<IReadOnlyList<SecretItem>>> GetItems(CancellationToken cancellationToken)
    {
        var items = await FetchItems(cancellationToken);

        return items is null
            ? Result<IReadOnlyList<SecretItem>>.Failure(ErrorCode.VaultwardenUnavailable.ToString())
            : Result<IReadOnlyList<SecretItem>>.Success(items.Select(item => new SecretItem(item.Id, item.Name)).ToList());
    }

    public async Task<Result<IReadOnlyDictionary<string, string>>> GetPasswords(
        IReadOnlyCollection<string> itemIds,
        CancellationToken cancellationToken)
    {
        var items = await FetchItems(cancellationToken);

        if (items is null)
        {
            return Result<IReadOnlyDictionary<string, string>>.Failure(ErrorCode.VaultwardenUnavailable.ToString());
        }

        var requestedIds = itemIds.ToHashSet(StringComparer.Ordinal);

        var passwords = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Id)
                || !requestedIds.Contains(item.Id)
                || string.IsNullOrWhiteSpace(item.Login?.Password)
                || passwords.ContainsKey(item.Id))
            {
                continue;
            }

            passwords[item.Id] = item.Login.Password;
        }

        return Result<IReadOnlyDictionary<string, string>>.Success(passwords);
    }

    private async Task<IReadOnlyList<VaultwardenListItem>?> FetchItems(CancellationToken cancellationToken)
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

    private static bool IsMissingItem(HttpResponseMessage response, VaultwardenResponse<VaultwardenItem>? result)
    {
        return (int)response.StatusCode < (int)HttpStatusCode.InternalServerError && result?.Success == false;
    }
}
