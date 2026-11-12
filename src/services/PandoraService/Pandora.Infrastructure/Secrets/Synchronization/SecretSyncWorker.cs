using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            _logger.LogInformation("Фоновая синхронизация секретов отключена в конфигурации");

            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.IntervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await SynchronizeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Сбой фоновой синхронизации секретов");
            }
        }
    }

    private async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var metadataRepository = scope.ServiceProvider.GetRequiredService<ISecretMetadataRepository>();
        var secretWriter = scope.ServiceProvider.GetRequiredService<ISecretWriter>();

        var pending = await metadataRepository.GetPendingSync(cancellationToken);

        if (pending.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Найдено записей для синхронизации: {Count}", pending.Count);

        foreach (var metadata in pending)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await SynchronizeItemAsync(metadata, secretWriter, metadataRepository, cancellationToken);
        }
    }

    private async Task SynchronizeItemAsync(
        SecretMetadata metadata,
        ISecretWriter secretWriter,
        ISecretMetadataRepository metadataRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            if (metadata.SyncOperation is SecretSyncOperation.Delete)
            {
                await SynchronizeDeleteAsync(metadata, secretWriter, metadataRepository, cancellationToken);

                return;
            }

            await SynchronizeUpdateAsync(metadata, secretWriter, metadataRepository, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось синхронизировать секрет {ItemId}", metadata.VaultItemId);
        }
    }

    private async Task SynchronizeDeleteAsync(
        SecretMetadata metadata,
        ISecretWriter secretWriter,
        ISecretMetadataRepository metadataRepository,
        CancellationToken cancellationToken)
    {
        var result = await secretWriter.Delete(metadata.VaultItemId, cancellationToken);

        if (result is SecretDeleteResult.GatewayError)
        {
            _logger.LogWarning("Повторное удаление секрета {ItemId} отклонено шлюзом", metadata.VaultItemId);

            return;
        }

        await metadataRepository.DeleteIfPending(metadata.VaultItemId, cancellationToken);

        _logger.LogInformation("Удаление секрета {ItemId} синхронизировано", metadata.VaultItemId);
    }

    private async Task SynchronizeUpdateAsync(
        SecretMetadata metadata,
        ISecretWriter secretWriter,
        ISecretMetadataRepository metadataRepository,
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
            _logger.LogWarning("Повторное обновление секрета {ItemId} отклонено шлюзом", metadata.VaultItemId);

            return;
        }

        if (result is SecretUpdateResult.NoPassword)
        {
            _logger.LogWarning(
                "Секрет {ItemId} синхронизирован без пароля: в Vaultwarden пароль отсутствует и не может быть восстановлен",
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
}
