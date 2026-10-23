using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using MediatR;
using Pandora.App.Features.Activation;
using Pandora.Core.Interfaces;
using Telegram.Bot.Types;

namespace Pandora.TgBot.BotHandlers;

public sealed class ContactBotHandler : IBotHandler
{
    private readonly ITelegramNotificationService _telegram;
    private readonly IMediator _mediator;
    private readonly ILocalizer _localizer;

    public ContactBotHandler(ITelegramNotificationService telegram, IMediator mediator, ILocalizer localizer)
    {
        _telegram = telegram;
        _mediator = mediator;
        _localizer = localizer;
    }

    public bool CanHandle(Update update)
    {
        return update.Message?.Contact is not null;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is { Chat.Id: long telegramUserId, MessageId: int messageId, Contact.PhoneNumber: string phone })
        {
            var result = await _mediator.Send(new SubmitPhoneMessage(telegramUserId, messageId, phone), cancellationToken);

            await _telegram.SendMessageAsync(telegramUserId, result.GetDisplayText(_localizer), result.ToTelegramMarkup(), cancellationToken);
        }
    }
}
