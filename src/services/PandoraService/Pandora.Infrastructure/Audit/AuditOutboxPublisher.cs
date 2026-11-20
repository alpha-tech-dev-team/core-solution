using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.Infrastructure.Database;

namespace Pandora.Infrastructure.Audit;

public sealed class AuditOutboxPublisher : BackgroundService
{
    private const int BatchSize = 100;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);
    private static readonly string SelectPendingSql =
        $"select * from {DatabaseConstants.AuditOutbox.Table} " +
        $"order by {DatabaseConstants.AuditOutbox.CreatedAt} " +
        $"limit {BatchSize} for update skip locked";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAuditNotifier _notifier;
    private readonly ILogger<AuditOutboxPublisher> _logger;

    public AuditOutboxPublisher(
        IServiceScopeFactory scopeFactory,
        IAuditNotifier notifier,
        ILogger<AuditOutboxPublisher> logger)
    {
        _scopeFactory = scopeFactory;
        _notifier = notifier;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (await PublishBatchAsync(stoppingToken))
                {
                    continue;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось опубликовать outbox аудита, повтор через {Delay}", RetryDelay);
            }

            try
            {
                await Task.Delay(RetryDelay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private async Task<bool> PublishBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<PandoraDbContext>();

        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);

        var messages = await database.AuditOutbox
            .FromSqlRaw(SelectPendingSql)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            await transaction.RollbackAsync(cancellationToken);

            return false;
        }

        foreach (var message in messages)
        {
            AuditEntryNotification? notification;
            try
            {
                notification = AuditNotificationSerializer.Deserialize(message.Payload)
                    ?? throw new JsonException("Пустой payload");
            }
            catch (JsonException exception)
            {
                _logger.LogError(exception, "Не удалось прочитать outbox-запись аудита {Id}, запись удаляется", message.Id);
                database.AuditOutbox.Remove(message);

                continue;
            }

            await _notifier.Notify(notification);
            database.AuditOutbox.Remove(message);
        }

        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }
}
