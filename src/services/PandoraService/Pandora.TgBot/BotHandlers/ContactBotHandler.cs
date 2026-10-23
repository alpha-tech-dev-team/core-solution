using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using MediatR;
using Pandora.App.Features.Activation;
using Pandora.Core.Interfaces;
using Telegram.Bot.Types;

namespace Pandora.TgBot.BotHandlers;

public sealed class ContactBotHandler : IBotHandler
{
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IMediator _mediator;
    private readonly ILocalizer _localizer;

    public ContactBotHandler(ITelegramNotificationService telegramNotificationService, IMediator mediator, ILocalizer localizer)
    {
        _telegramNotificationService = telegramNotificationService;
        _mediator = mediator;
        _localizer = localizer;
    }

    public bool CanHandle(Update update)
    {
        return update.Message?.Contact is not null;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is { Chat.Id: long tgUserId, MessageId: int messageId, Contact.PhoneNumber: string phone })
        {
            var result = await _mediator.Send(new SubmitPhoneMessage(tgUserId, messageId, phone), cancellationToken);

            await _telegramNotificationService.SendMessageAsync(tgUserId, result.GetDisplayText(_localizer), result.ToTelegramMarkup(), cancellationToken);
        }
    }
}
