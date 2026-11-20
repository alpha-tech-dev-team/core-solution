using Microsoft.Extensions.Logging;
using Pandora.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace Pandora.TgBot.Credentials;

public sealed class TelegramCredentialMessageGateway : ICredentialMessageGateway
{
    private const string NotFoundMarker = "not found";
    private const string NotModifiedMarker = "not modified";

    private readonly ITelegramBotClient _client;
    private readonly ILogger<TelegramCredentialMessageGateway> _logger;

    public TelegramCredentialMessageGateway(
        ITelegramBotClient client,
        ILogger<TelegramCredentialMessageGateway> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<CredentialMessageOutcome> Delete(
        long chatId,
        int messageId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _client.DeleteMessage(chatId, messageId, cancellationToken);

            return CredentialMessageOutcome.Succeeded;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (ApiRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "Telegram отклонил удаление сообщения {MessageId}",
                messageId);

            return Classify(exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Не удалось удалить сообщение {MessageId}",
                messageId);

            return CredentialMessageOutcome.Failed;
        }
    }

    public async Task<CredentialMessageOutcome> ReplaceText(
        long chatId,
        int messageId,
        string text,
        CancellationToken cancellationToken)
    {
        try
        {
            await _client.EditMessageText(
                chatId: chatId,
                messageId: messageId,
                text: text,
                cancellationToken: cancellationToken);

            return CredentialMessageOutcome.Succeeded;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (ApiRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "Telegram отклонил замену текста сообщения {MessageId}",
                messageId);

            if (exception.Message.Contains(NotModifiedMarker, StringComparison.OrdinalIgnoreCase))
            {
                return CredentialMessageOutcome.Succeeded;
            }

            return Classify(exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Не удалось заменить текст сообщения {MessageId}",
                messageId);

            return CredentialMessageOutcome.Failed;
        }
    }

    private static CredentialMessageOutcome Classify(string message)
    {
        return message.Contains(NotFoundMarker, StringComparison.OrdinalIgnoreCase)
            ? CredentialMessageOutcome.NotFound
            : CredentialMessageOutcome.Failed;
    }
}
