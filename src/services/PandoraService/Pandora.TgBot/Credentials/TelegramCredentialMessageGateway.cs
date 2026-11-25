using System.Net;
using Microsoft.Extensions.Logging;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace Pandora.TgBot.Credentials;

public sealed class TelegramCredentialMessageGateway : ICredentialMessageGateway
{
    private const string ErrorDescriptionPrefix = "Bad Request: ";
    private const string NotModifiedDescription = "message is not modified";
    private const string DeleteMessageNotFoundDescription = "message to delete not found";
    private const string EditMessageNotFoundDescription = "message to edit not found";

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
            _logger.LogWarning(exception, "Telegram отклонил удаление сообщения {MessageId}", messageId);

            return ClassifyDeleteError(exception);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Не удалось удалить сообщение {MessageId}", messageId);

            return CredentialMessageOutcome.Failed;
        }
    }

    public async Task<CredentialMessageOutcome> ReplaceText(long chatId, int messageId, string text, CancellationToken token)
    {
        try
        {
            await _client.EditMessageText(chatId: chatId, messageId: messageId, text: text, cancellationToken: token);

            return CredentialMessageOutcome.Succeeded;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
        catch (ApiRequestException exception)
        {
            _logger.LogWarning(exception, "Telegram отклонил замену текста сообщения {MessageId}", messageId);

            if (IsNotModified(exception))
            {
                return CredentialMessageOutcome.Succeeded;
            }

            return ClassifyEditError(exception);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Не удалось заменить текст сообщения {MessageId}", messageId);

            return CredentialMessageOutcome.Failed;
        }
    }

    private static CredentialMessageOutcome ClassifyDeleteError(ApiRequestException exception)
    {
        return IsExactBadRequest(exception, DeleteMessageNotFoundDescription)
            ? CredentialMessageOutcome.NotFound
            : CredentialMessageOutcome.Failed;
    }

    private static CredentialMessageOutcome ClassifyEditError(ApiRequestException exception)
    {
        return IsExactBadRequest(exception, EditMessageNotFoundDescription)
            ? CredentialMessageOutcome.NotFound
            : CredentialMessageOutcome.Failed;
    }

    private static bool IsNotModified(ApiRequestException exception)
    {
        return IsExactBadRequest(exception, NotModifiedDescription);
    }

    private static bool IsExactBadRequest(ApiRequestException exception, string description)
    {
        var normalizedDescription = exception.Message.StartsWith(ErrorDescriptionPrefix, StringComparison.OrdinalIgnoreCase)
            ? exception.Message[ErrorDescriptionPrefix.Length..]
            : exception.Message;

        return exception.ErrorCode == (int)HttpStatusCode.BadRequest
            && string.Equals(normalizedDescription, description, StringComparison.OrdinalIgnoreCase);
    }
}
