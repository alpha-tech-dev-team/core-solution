using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.Domain.Options;

namespace Pandora.TgBot.Application.Features.Credentials;

public sealed class CredentialMessageCleanupService
{
    private readonly ICredentialAccessMessageRepository _messageRepository;
    private readonly ICredentialMessageGateway _gateway;
    private readonly ICredentialMessageNotifier _notifier;
    private readonly CredentialAccessNoticeBuilder _noticeBuilder;
    private readonly IOptions<CredentialCleanupOptions> _options;
    private readonly ILogger<CredentialMessageCleanupService> _logger;

    public CredentialMessageCleanupService(
        ICredentialAccessMessageRepository messageRepository,
        ICredentialMessageGateway gateway,
        ICredentialMessageNotifier notifier,
        CredentialAccessNoticeBuilder noticeBuilder,
        IOptions<CredentialCleanupOptions> options,
        ILogger<CredentialMessageCleanupService> logger)
    {
        _messageRepository = messageRepository;
        _gateway = gateway;
        _notifier = notifier;
        _noticeBuilder = noticeBuilder;
        _options = options;
        _logger = logger;
    }

    public async Task<int> ProcessDueAsync(DateTime now, CancellationToken cancellationToken)
    {
        var options = _options.Value;
        var claimUntil = now.AddSeconds(CredentialAccessConstants.QueueClaimDurationSeconds);

        await RemoveCompletedAsync(now, options, cancellationToken);

        var dueMessages = await _messageRepository.ClaimDue(
            new CredentialMessageClaim(now, claimUntil, options.BatchSize, options.MaxAttempts),
            cancellationToken);

        foreach (var message in dueMessages)
        {
            await ProcessMessageAsync(message, options, cancellationToken);
        }

        return dueMessages.Count;
    }

    private async Task RemoveCompletedAsync(
        DateTime now,
        CredentialCleanupOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var retentionBefore = now.AddHours(-options.CompletedRetentionHours);

            await _messageRepository.DeleteCompleted(retentionBefore, cancellationToken);
            await _messageRepository.DeleteExhausted(options.MaxAttempts, retentionBefore, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(exception, "Не удалось удалить обработанные записи очереди очистки");
        }
    }

    private async Task ProcessMessageAsync(
        CredentialAccessMessage message,
        CredentialCleanupOptions options,
        CancellationToken cancellationToken)
    {
        var notice = _noticeBuilder.Build(message.ItemName, message.RequestedAt);

        if (message.DeletedAt is null)
        {
            var outcome = await _gateway.Delete(message.ChatId, message.MessageId, cancellationToken);

            if (outcome == CredentialMessageOutcome.Failed)
            {
                await HandleDeleteFailureAsync(message, notice, options, cancellationToken);

                return;
            }

            if (outcome == CredentialMessageOutcome.NotFound)
            {
                await _messageRepository.MarkDeleted(message.Id, DateTime.UtcNow, DateTime.UtcNow, cancellationToken);

                return;
            }

            await _messageRepository.MarkDeleted(message.Id, DateTime.UtcNow, null, cancellationToken);
        }

        if (message.NotifiedAt is not null)
        {
            return;
        }

        await SendNoticeAsync(message, notice, options, cancellationToken);
    }

    private async Task HandleDeleteFailureAsync(
        CredentialAccessMessage message,
        string notice,
        CredentialCleanupOptions options,
        CancellationToken cancellationToken)
    {
        if (message.AttemptNumber >= options.DeleteAttemptsBeforeFallback)
        {
            var fallback = await _gateway.ReplaceText(
                message.ChatId,
                message.MessageId,
                notice,
                cancellationToken);

            if (fallback != CredentialMessageOutcome.Failed)
            {
                _logger.LogWarning("Сообщение не удалось удалить, учётные данные заменены уведомлением {MessageId}", message.MessageId);

                await _messageRepository.MarkDeleted(message.Id, DateTime.UtcNow, DateTime.UtcNow, cancellationToken);

                return;
            }
        }

        await MarkFailedAsync(message, CredentialCleanupErrors.MessageDeleteFailed, options, cancellationToken);
    }

    private async Task SendNoticeAsync(
        CredentialAccessMessage message,
        string notice,
        CredentialCleanupOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            await _notifier.SendNotice(message.ChatId, notice, cancellationToken);

            await _messageRepository.MarkNotified(message.Id, DateTime.UtcNow, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Не удалось отправить уведомление об истечении доступа для сообщения {MessageId}", message.MessageId);

            await MarkFailedAsync(message, CredentialCleanupErrors.NoticeSendFailed, options, cancellationToken);
        }
    }

    private async Task MarkFailedAsync(
        CredentialAccessMessage message,
        string error,
        CredentialCleanupOptions options,
        CancellationToken cancellationToken)
    {
        var exponent = Math.Min(message.Attempts, CredentialAccessConstants.MaxBackoffExponent);
        var delaySeconds = Math.Min(
            options.RetryBaseDelaySeconds * Math.Pow(CredentialAccessConstants.BackoffBase, exponent),
            options.MaxRetryDelaySeconds);
        var nextAttemptAt = DateTime.UtcNow.AddSeconds(delaySeconds);

        var updated = await _messageRepository.MarkFailed(
            message.Id,
            new CredentialMessageFailure(message.Attempts, error, nextAttemptAt),
            cancellationToken);

        if (!updated)
        {
            _logger.LogWarning("Запись очереди очистки изменилась параллельно, повтор не запланирован {MessageId}", message.MessageId);

            return;
        }

        if (message.AttemptNumber >= options.MaxAttempts)
        {
            _logger.LogError("Исчерпаны попытки очистки сообщения {MessageId} в чате {ChatId}: {Error}", message.MessageId, message.ChatId, error);
        }
    }
}
