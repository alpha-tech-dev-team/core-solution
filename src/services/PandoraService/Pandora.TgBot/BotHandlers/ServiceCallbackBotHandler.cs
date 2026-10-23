using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using MediatR;
using Pandora.App.Common;
using Pandora.App.Features.Credentials;
using Pandora.Core.Interfaces;
using Telegram.Bot.Types;

namespace Pandora.TgBot.BotHandlers;

public sealed class ServiceCallbackBotHandler : IBotHandler
{
    private readonly ITelegramNotificationService _telegram;
    private readonly IMediator _mediator;
    private readonly ILocalizer _localizer;

    public ServiceCallbackBotHandler(ITelegramNotificationService telegram, IMediator mediator, ILocalizer localizer)
    {
        _telegram = telegram;
        _mediator = mediator;
        _localizer = localizer;
    }

    public bool CanHandle(Update update)
    {
        return update.CallbackQuery?.Data?.StartsWith(CallbackConstants.ServicePrefix, StringComparison.Ordinal) is true;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is { Data: string data, Message: { Chat.Id: long telegramUserId, MessageId: int messageId } })
        {
            var serviceName = data[CallbackConstants.ServicePrefix.Length..];
            var result = await _mediator.Send(new GetCredentialsMessage(telegramUserId, messageId, serviceName), cancellationToken);

            await _telegram.SendMessageAsync(telegramUserId, result.GetDisplayText(_localizer), result.ToTelegramMarkup(), cancellationToken);
        }
    }
}
