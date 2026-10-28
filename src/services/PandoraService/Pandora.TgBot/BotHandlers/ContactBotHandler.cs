using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using MediatR;
using Pandora.App.Features.Activation;
using Telegram.Bot.Types;

namespace Pandora.TgBot.BotHandlers;

public sealed class ContactBotHandler : IBotHandler
{
    private const string StartCommand = "/start";

    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IMediator _mediator;

    public ContactBotHandler(ITelegramNotificationService telegramNotificationService, IMediator mediator)
    {
        _telegramNotificationService = telegramNotificationService;
        _mediator = mediator;
    }

    public bool CanHandle(Update update)
    {
        return update.Message?.Contact is not null || update.Message?.Text is StartCommand;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is { Chat.Id: long tgUserId, MessageId: int messageId, Contact.PhoneNumber: string phone })
        {
            var result = await _mediator.Send(new SubmitPhoneMessage(tgUserId, messageId, phone), cancellationToken);

            await _telegramNotificationService.SendMessageAsync(tgUserId, result.GetDisplayText(), result.ToTelegramMarkup(), cancellationToken);

            return;
        }

        if (update.Message is { Chat.Id: long startTgUserId, MessageId: int startMessageId })
        {
            var result = await _mediator.Send(new SubmitPhoneMessage(startTgUserId, startMessageId, null), cancellationToken);

            await _telegramNotificationService.SendMessageAsync(startTgUserId, result.GetDisplayText(), result.ToTelegramMarkup(), cancellationToken);
        }
    }
}
