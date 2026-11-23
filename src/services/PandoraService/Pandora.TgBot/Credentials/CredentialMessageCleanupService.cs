using CoreSolution.Shared.TgBot.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pandora.Domain.Entities;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Options;

namespace Pandora.TgBot.Credentials;

public sealed class CredentialMessageCleanupService
{
    private const int MaxBackoffExponent = 10;
    private const int BackoffBase = 2;

    private readonly ICredentialAccessMessageRepository _messageRepository;
    private readonly ICredentialMessageGateway _gateway;
    private readonly ITelegramNotificationService _notificationService;
    private readonly CredentialAccessNoticeBuilder _noticeBuilder;
    private readonly IOptions<CredentialCleanupOptions> _options;
    private readonly ILogger<CredentialMessageCleanupService> _logger;

    public CredentialMessageCleanupService(
        ICredentialAccessMessageRepository messageRepository,
        ICredentialMessageGateway gateway,
        ITelegramNotificationService notificationService,
        CredentialAccessNoticeBuilder noticeBuilder,
        IOptions<CredentialCleanupOptions> options,
        ILogger<CredentialMessageCleanupService> logger)
    {
        _messageRepository = messageRepository;
        _gateway = gateway;
        _notificationService = notificationService;
        _noticeBuilder = noticeBuilder;
        _options = options;
        _logger = logger;
    }

    public async Task<int> ProcessDueAsync(DateTime now, CancellationToken cancellationToken)
    {
        var options = _options.Value;

        await RemoveCompletedAsync(now, options, cancellationToken);

        var dueMessages = await _messageRepository.GetDue(
            now,
            options.BatchSize,
            query => query.OrderBy(message => message.NextAttemptAt).ThenBy(message => message.Id),
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
                await _messageRepository.MarkDeleted(
                    message.Id,
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    cancellationToken);

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
        var attemptNumber = message.AttemptNumber;

        if (attemptNumber >= options.DeleteAttemptsBeforeFallback)
        {
            var fallback = await _gateway.ReplaceText(
                message.ChatId,
                message.MessageId,
                notice,
                cancellationToken);

            if (fallback != CredentialMessageOutcome.Failed)
            {
                _logger.LogWarning(
                    "Сообщение {MessageId} не удалось удалить, учётные данные заменены уведомлением",
                    message.MessageId);

                await _messageRepository.MarkDeleted(
                    message.Id,
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    cancellationToken);

                return;
            }
        }

        await MarkFailedAsync(
            message,
            $"Не удалось удалить сообщение {message.MessageId}",
            options,
            cancellationToken);
    }

    private async Task SendNoticeAsync(
        CredentialAccessMessage message,
        string notice,
        CredentialCleanupOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            await _notificationService.SendMessageAsync(message.ChatId, notice, null, cancellationToken);

            await _messageRepository.MarkNotified(message.Id, DateTime.UtcNow, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Не удалось отправить уведомление об истечении доступа для сообщения {MessageId}",
                message.MessageId);

            await MarkFailedAsync(
                message,
                exception.Message,
                options,
                cancellationToken);
        }
    }

    private async Task MarkFailedAsync(
        CredentialAccessMessage message,
        string error,
        CredentialCleanupOptions options,
        CancellationToken cancellationToken)
    {
        var exponent = Math.Min(message.Attempts, MaxBackoffExponent);
        var delaySeconds = Math.Min(
            options.RetryBaseDelaySeconds * Math.Pow(BackoffBase, exponent),
            options.MaxRetryDelaySeconds);
        var nextAttemptAt = DateTime.UtcNow.AddSeconds(delaySeconds);

        await _messageRepository.MarkFailed(message.Id, error, nextAttemptAt, cancellationToken);
    }
}
