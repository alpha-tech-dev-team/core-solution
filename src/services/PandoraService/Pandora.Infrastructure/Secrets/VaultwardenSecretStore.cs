using System.Net.Http.Json;
using System.Text.Json;
using CoreSolution.Shared.Results;
using Microsoft.Extensions.Logging;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Infrastructure.Secrets;

public sealed class VaultwardenSecretStore : ISecretStore
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

            // 4xx с семантическим ответом шлюза (success: false) — элемента нет в хранилище, остальное — сбой шлюза
            return (int)response.StatusCode < 500 && result?.Success == false
                ? Result<Secret>.Failure(ErrorCode.SecretNotFound.ToString())
                : Result<Secret>.Failure(ErrorCode.VaultwardenUnavailable.ToString());
        }

        var itemDetails = result?.Data;

        if (string.IsNullOrWhiteSpace(itemDetails?.Login?.Password))
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
