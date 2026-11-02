using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.Extensions;
using CoreSolution.Shared.Application.Messages;
using MediatR;
using Telegram.Bot.Types;

namespace CoreSolution.Shared.TgBot.BotHandlers;

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
        if (update.Message is not { Chat.Id: long tgUserId, MessageId: int messageId })
        {
            return;
        }

        var phone = update.Message.Contact?.PhoneNumber;

        var result = await _mediator.Send(new SubmitPhoneMessage(tgUserId, messageId, phone), cancellationToken);

        await _telegramNotificationService.SendMessageAsync(tgUserId, result.GetDisplayText(), result.ToTelegramMarkup(), cancellationToken);
    }
}
