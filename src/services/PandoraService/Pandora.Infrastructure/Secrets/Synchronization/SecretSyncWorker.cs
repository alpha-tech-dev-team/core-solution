using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.Domain.Options;

namespace Pandora.Infrastructure.Secrets.Synchronization;

public sealed class SecretSyncWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<SecretSyncOptions> _options;
    private readonly ILogger<SecretSyncWorker> _logger;

    public SecretSyncWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<SecretSyncOptions> options,
        ILogger<SecretSyncWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = _options.Value;

        if (!options.Enabled)
        {
            _logger.LogInformation("Синхронизация секретов отключена в конфигурации");

            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var started = DateTime.UtcNow;

            try
            {
                await SynchronizeBatchAsync(options, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Сбой цикла синхронизации секретов");
            }

            var elapsed = DateTime.UtcNow - started;
            var remaining = TimeSpan.FromSeconds(options.IntervalSeconds) - elapsed;

            if (remaining <= TimeSpan.Zero)
            {
                continue;
            }

            try
            {
                await Task.Delay(remaining, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private async Task SynchronizeBatchAsync(SecretSyncOptions options, CancellationToken cancellationToken)
    {
        var query = new SecretSyncBatchQuery(options.BatchSize, options.MaxAttempts, DateTime.UtcNow);

        IReadOnlyList<SecretMetadata> pending;

        using (var scope = _scopeFactory.CreateScope())
        {
            var metadataRepository = scope.ServiceProvider.GetRequiredService<ISecretMetadataRepository>();

            pending = await metadataRepository.GetPendingSync(query, cancellationToken);
        }

        if (pending.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Найдено секретов для синхронизации: {Count}", pending.Count);

        using var throttler = new SemaphoreSlim(options.MaxParallelism);

        var tasks = pending.Select(metadata => SynchronizeItemAsync(metadata, options, throttler, cancellationToken));

        await Task.WhenAll(tasks);
    }

    private async Task SynchronizeItemAsync(
        SecretMetadata metadata,
        SecretSyncOptions options,
        SemaphoreSlim throttler,
        CancellationToken cancellationToken)
    {
        await throttler.WaitAsync(cancellationToken);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var metadataRepository = scope.ServiceProvider.GetRequiredService<ISecretMetadataRepository>();
            var secretWriter = scope.ServiceProvider.GetRequiredService<ISecretWriter>();

            if (metadata.SyncOperation is SecretSyncOperation.Delete)
            {
                await SynchronizeDeleteAsync(metadata, secretWriter, metadataRepository, options, cancellationToken);

                return;
            }

            await SynchronizeUpdateAsync(metadata, secretWriter, metadataRepository, options, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось синхронизировать секрет {ItemId}", metadata.VaultItemId);

            await TryMarkFailedAsync(metadata, exception.Message, options, cancellationToken);
        }
        finally
        {
            throttler.Release();
        }
    }

    private async Task SynchronizeDeleteAsync(
        SecretMetadata metadata,
        ISecretWriter secretWriter,
        ISecretMetadataRepository metadataRepository,
        SecretSyncOptions options,
        CancellationToken cancellationToken)
    {
        var result = await secretWriter.Delete(metadata.VaultItemId, cancellationToken);

        if (result is SecretDeleteResult.GatewayError)
        {
            _logger.LogWarning("Шлюз Vaultwarden отклонил удаление секрета {ItemId}, будет повторная попытка", metadata.VaultItemId);

            await MarkFailedAsync(metadata, nameof(SecretDeleteResult.GatewayError), metadataRepository, options, cancellationToken);

            return;
        }

        await metadataRepository.DeleteIfPending(metadata.VaultItemId, cancellationToken);

        _logger.LogInformation("Удаление секрета {ItemId} синхронизировано", metadata.VaultItemId);
    }

    private async Task SynchronizeUpdateAsync(
        SecretMetadata metadata,
        ISecretWriter secretWriter,
        ISecretMetadataRepository metadataRepository,
        SecretSyncOptions options,
        CancellationToken cancellationToken)
    {
        var result = await secretWriter.Update(
            metadata.VaultItemId,
            new SecretUpdateData(metadata.ResourceName, metadata.Username, null),
            cancellationToken);

        if (result is SecretUpdateResult.NotFound)
        {
            await metadataRepository.DeleteIfPending(metadata.VaultItemId, cancellationToken);

            _logger.LogWarning("Секрет {ItemId} отсутствует в Vaultwarden, запись метаданных удалена", metadata.VaultItemId);

            return;
        }

        if (result is SecretUpdateResult.GatewayError)
        {
            _logger.LogWarning("Шлюз Vaultwarden отклонил обновление секрета {ItemId}, будет повторная попытка", metadata.VaultItemId);

            await MarkFailedAsync(metadata, nameof(SecretUpdateResult.GatewayError), metadataRepository, options, cancellationToken);

            return;
        }

        if (result is SecretUpdateResult.NoPassword)
        {
            _logger.LogWarning(
                "Секрет {ItemId} синхронизирован без пароля: в Vaultwarden пароль отсутствует и не был передан",
                metadata.VaultItemId);
        }

        var cleared = await metadataRepository.ClearSyncStateIfUnchanged(
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

    private async Task MarkFailedAsync(
        SecretMetadata metadata,
        string error,
        ISecretMetadataRepository metadataRepository,
        SecretSyncOptions options,
        CancellationToken cancellationToken)
    {
        var attempt = metadata.AttemptCount + 1;
        var nextAttemptAt = DateTime.UtcNow.Add(CalculateDelay(attempt, options));

        await metadataRepository.MarkSyncAttemptFailed(metadata.VaultItemId, error, nextAttemptAt, cancellationToken);

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

    private async Task TryMarkFailedAsync(
        SecretMetadata metadata,
        string error,
        SecretSyncOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var metadataRepository = scope.ServiceProvider.GetRequiredService<ISecretMetadataRepository>();

            await MarkFailedAsync(metadata, error, metadataRepository, options, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось зафиксировать ошибку синхронизации секрета {ItemId}", metadata.VaultItemId);
        }
    }

    private static TimeSpan CalculateDelay(int attempt, SecretSyncOptions options)
    {
        var exponent = Math.Min(attempt - 1, SecretSyncLimits.MaxBackoffExponent);
        var seconds = (long)options.IntervalSeconds << exponent;

        return TimeSpan.FromSeconds(Math.Min(seconds, options.MaxRetryDelaySeconds));
    }
}
