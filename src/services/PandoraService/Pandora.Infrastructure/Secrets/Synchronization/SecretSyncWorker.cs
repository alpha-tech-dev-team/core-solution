using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pandora.Domain.Entities;
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
        IReadOnlyList<SecretMetadata> pending;

        using (var scope = _scopeFactory.CreateScope())
        {
            var syncService = scope.ServiceProvider.GetRequiredService<SecretSyncService>();

            pending = await syncService.GetPendingAsync(cancellationToken);
        }

        if (pending.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Найдено секретов для синхронизации: {Count}", pending.Count);

        using var throttler = new SemaphoreSlim(options.MaxParallelism);

        var tasks = pending.Select(metadata => SynchronizeItemAsync(metadata, throttler, cancellationToken));

        await Task.WhenAll(tasks);
    }

    private async Task SynchronizeItemAsync(
        SecretMetadata metadata,
        SemaphoreSlim throttler,
        CancellationToken cancellationToken)
    {
        await throttler.WaitAsync(cancellationToken);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<SecretSyncService>();

            await syncService.SynchronizeAsync(metadata, cancellationToken);
        }
        finally
        {
            throttler.Release();
        }
    }
}
