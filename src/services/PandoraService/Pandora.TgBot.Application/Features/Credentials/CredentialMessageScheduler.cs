using Microsoft.Extensions.Logging;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;

namespace Pandora.TgBot.Application.Features.Credentials;

public sealed class CredentialMessageScheduler
{
    private readonly ICredentialAccessSettingsRepository _settingsRepository;
    private readonly ICredentialAccessMessageRepository _messageRepository;
    private readonly ISecretMetadataRepository _metadataRepository;
    private readonly ICredentialMessageGateway _gateway;
    private readonly CredentialAccessNoticeBuilder _noticeBuilder;
    private readonly ILogger<CredentialMessageScheduler> _logger;

    public CredentialMessageScheduler(
        ICredentialAccessSettingsRepository settingsRepository,
        ICredentialAccessMessageRepository messageRepository,
        ISecretMetadataRepository metadataRepository,
        ICredentialMessageGateway gateway,
        CredentialAccessNoticeBuilder noticeBuilder,
        ILogger<CredentialMessageScheduler> logger)
    {
        _settingsRepository = settingsRepository;
        _messageRepository = messageRepository;
        _metadataRepository = metadataRepository;
        _gateway = gateway;
        _noticeBuilder = noticeBuilder;
        _logger = logger;
    }

    public async Task<bool> ScheduleAsync(
        long chatId,
        int messageId,
        string itemId,
        CancellationToken cancellationToken)
    {
        var requestedAt = DateTime.UtcNow;

        try
        {
            var ttlMinutes = await ResolveTtlMinutesAsync(cancellationToken);
            var itemName = await ResolveItemNameAsync(itemId, cancellationToken);
            var expiresAt = requestedAt.AddMinutes(ttlMinutes);

            var message = new CredentialAccessMessage(
                chatId,
                messageId,
                Truncate(itemId, CredentialAccessLimits.ItemIdMaxLength),
                Truncate(itemName, CredentialAccessLimits.ItemNameMaxLength),
                requestedAt,
                expiresAt);

            await _messageRepository.Add(message, cancellationToken);

            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Не удалось удалить сообщение {MessageId} при отмене: чат {ChatId}, сервис {ItemId}, {RequestedAt}",
                messageId,
                chatId,
                itemId,
                requestedAt);

            return false;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Не удалось запланировать удаление сообщения {MessageId}", messageId);

            await RemoveImmediatelyAsync(chatId, messageId, itemId, requestedAt, cancellationToken);

            return false;
        }
    }

    private async Task<int> ResolveTtlMinutesAsync(CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.Get(cancellationToken);

        return CredentialAccessConstants.AllowedTtlMinutes.Contains(settings.TtlMinutes)
            ? settings.TtlMinutes
            : CredentialAccessConstants.DefaultTtlMinutes;
    }

    private async Task<string> ResolveItemNameAsync(string itemId, CancellationToken cancellationToken)
    {
        try
        {
            var metadata = await _metadataRepository.GetById(itemId, cancellationToken);

            return metadata?.ResourceName ?? itemId;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(exception, "Не удалось получить название сервиса {ItemId}, используется идентификатор", itemId);

            return itemId;
        }
    }

    private async Task RemoveImmediatelyAsync(
        long chatId,
        int messageId,
        string itemId,
        DateTime requestedAt,
        CancellationToken cancellationToken)
    {
        if (await TryDeleteAsync(chatId, messageId, cancellationToken))
        {
            return;
        }

        await TryReplaceAsync(chatId, messageId, itemId, requestedAt, cancellationToken);
    }

    private async Task<bool> TryDeleteAsync(long chatId, int messageId, CancellationToken cancellationToken)
    {
        try
        {
            var outcome = await _gateway.Delete(chatId, messageId, cancellationToken);

            if (outcome == CredentialMessageOutcome.Succeeded)
            {
                return true;
            }

            _logger.LogWarning("Немедленное удаление сообщения {MessageId} не удалось: {Outcome}", messageId, outcome);

            return false;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Немедленное удаление сообщения {MessageId} завершилось ошибкой", messageId);

            return false;
        }
    }

    private async Task TryReplaceAsync(
        long chatId,
        int messageId,
        string itemId,
        DateTime requestedAt,
        CancellationToken cancellationToken)
    {
        try
        {
            var notice = _noticeBuilder.Build(itemId, requestedAt);
            var outcome = await _gateway.ReplaceText(chatId, messageId, notice, cancellationToken);

            if (outcome == CredentialMessageOutcome.Failed)
            {
                _logger.LogError("Не удалось заменить учётные данные в сообщении {MessageId}", messageId);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Замена учётных данных в сообщении {MessageId} завершилась ошибкой", messageId);
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
