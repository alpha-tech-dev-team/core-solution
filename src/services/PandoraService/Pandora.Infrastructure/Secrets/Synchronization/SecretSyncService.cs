using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.Domain.Options;

namespace Pandora.Infrastructure.Secrets.Synchronization;

public sealed class SecretSyncService
{
    private readonly ISecretMetadataRepository _metadataRepository;
    private readonly ISecretWriter _secretWriter;
    private readonly IOptions<SecretSyncOptions> _options;
    private readonly ILogger<SecretSyncService> _logger;

    public SecretSyncService(
        ISecretMetadataRepository metadataRepository,
        ISecretWriter secretWriter,
        IOptions<SecretSyncOptions> options,
        ILogger<SecretSyncService> logger)
    {
        _metadataRepository = metadataRepository;
        _secretWriter = secretWriter;
        _options = options;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SecretMetadata>> GetPendingAsync(CancellationToken cancellationToken)
    {
        var options = _options.Value;
        var query = new SecretSyncBatchQuery(options.BatchSize, options.MaxAttempts, DateTime.UtcNow);

        return await _metadataRepository.GetPendingSync(query, cancellationToken);
    }

    public async Task SynchronizeAsync(SecretMetadata metadata, CancellationToken cancellationToken)
    {
        try
        {
            if (metadata.SyncOperation is SecretSyncOperation.Delete)
            {
                await SynchronizeDeleteAsync(metadata, cancellationToken);

                return;
            }

            await SynchronizeUpdateAsync(metadata, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось синхронизировать секрет {ItemId}", metadata.VaultItemId);

            await MarkFailedAsync(metadata, exception.Message, cancellationToken);
        }
    }

    private async Task SynchronizeDeleteAsync(SecretMetadata metadata, CancellationToken cancellationToken)
    {
        var result = await _secretWriter.Delete(metadata.VaultItemId, cancellationToken);

        if (result is SecretDeleteResult.GatewayError)
        {
            _logger.LogWarning("Шлюз Vaultwarden отклонил удаление секрета {ItemId}, будет повторная попытка", metadata.VaultItemId);

            await MarkFailedAsync(metadata, nameof(SecretDeleteResult.GatewayError), cancellationToken);

            return;
        }

        await _metadataRepository.DeleteIfPending(metadata.VaultItemId, cancellationToken);

        _logger.LogInformation("Удаление секрета {ItemId} синхронизировано", metadata.VaultItemId);
    }

    private async Task SynchronizeUpdateAsync(SecretMetadata metadata, CancellationToken cancellationToken)
    {
        var result = await _secretWriter.Update(
            metadata.VaultItemId,
            new SecretUpdateData(metadata.ResourceName, metadata.Username, null),
            cancellationToken);

        if (result is SecretUpdateResult.NotFound)
        {
            await _metadataRepository.DeleteIfPending(metadata.VaultItemId, cancellationToken);

            _logger.LogWarning("Секрет {ItemId} отсутствует в Vaultwarden, запись метаданных удалена", metadata.VaultItemId);

            return;
        }

        if (result is SecretUpdateResult.GatewayError)
        {
            _logger.LogWarning("Шлюз Vaultwarden отклонил обновление секрета {ItemId}, будет повторная попытка", metadata.VaultItemId);

            await MarkFailedAsync(metadata, nameof(SecretUpdateResult.GatewayError), cancellationToken);

            return;
        }

        if (result is SecretUpdateResult.NoPassword)
        {
            _logger.LogWarning(
                "Секрет {ItemId} синхронизирован без пароля: в Vaultwarden пароль отсутствует и не был передан",
                metadata.VaultItemId);
        }

        var cleared = await _metadataRepository.ClearSyncStateIfUnchanged(
            metadata.VaultItemId,
            metadata.ResourceName,
            metadata.Username,
            cancellationToken);

        if (!cleared)
        {
            _logger.LogInformation("Секрет {ItemId} изменён во время синхронизации, остаётся в очереди", metadata.VaultItemId);

            return;
        }

        _logger.LogInformation("Обновление секрета {ItemId} синхронизировано", metadata.VaultItemId);
    }

    private async Task MarkFailedAsync(SecretMetadata metadata, string error, CancellationToken cancellationToken)
    {
        var options = _options.Value;
        var attempt = metadata.AttemptCount + 1;
        var nextAttemptAt = DateTime.UtcNow.Add(CalculateDelay(attempt, options));

        await _metadataRepository.MarkSyncAttemptFailed(metadata.VaultItemId, error, nextAttemptAt, cancellationToken);

        if (attempt >= options.MaxAttempts)
        {
            _logger.LogError(
                "Секрет {ItemId} исчерпал {Attempts} попыток синхронизации и больше не будет опрашиваться",
                metadata.VaultItemId,
                options.MaxAttempts);

            return;
        }

        _logger.LogWarning(
            "Секрет {ItemId} будет повторно синхронизирован после {NextAttemptAt:u}",
            metadata.VaultItemId,
            nextAttemptAt);
    }

    private static TimeSpan CalculateDelay(int attempt, SecretSyncOptions options)
    {
        var exponent = Math.Min(attempt - 1, SecretSyncLimits.MaxBackoffExponent);
        var seconds = (long)options.IntervalSeconds << exponent;

        return TimeSpan.FromSeconds(Math.Min(seconds, options.MaxRetryDelaySeconds));
    }
}
