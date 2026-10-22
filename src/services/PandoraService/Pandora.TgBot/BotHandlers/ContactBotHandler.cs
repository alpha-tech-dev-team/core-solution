using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using MediatR;
using Pandora.App.Features.Activation;
using Telegram.Bot.Types;

namespace Pandora.TgBot.BotHandlers;

public sealed class ContactBotHandler : IBotHandler
{
    private readonly ITelegramNotificationService _telegram;
    private readonly IMediator _mediator;

    public ContactBotHandler(ITelegramNotificationService telegram, IMediator mediator)
    {
        _telegram = telegram;
        _mediator = mediator;
    }

    public bool CanHandle(Update update)
    {
        return update.Message?.Contact is not null;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is { Chat.Id: long telegramUserId, MessageId: int messageId, Contact.PhoneNumber: string phone })
        {
            var result = await _mediator.Send(new SubmitPhoneMessage(telegramUserId, messageId, NormalizePhone(phone)), cancellationToken);

            await _telegram.SendMessageAsync(telegramUserId, result.GetTextOrErrors(), result.ToTelegramMarkup(), cancellationToken);
        }
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        return $"+{digits}";
    }
}
