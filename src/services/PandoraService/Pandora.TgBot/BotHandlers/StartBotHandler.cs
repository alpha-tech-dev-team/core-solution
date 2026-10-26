using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using MediatR;
using Pandora.App.Features.Start;
using Pandora.TgBot.Constants;
using Telegram.Bot.Types;

namespace Pandora.TgBot.BotHandlers;

public sealed class StartBotHandler : IBotHandler
{
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IMediator _mediator;

    public StartBotHandler(ITelegramNotificationService telegramNotificationService, IMediator mediator)
    {
        _telegramNotificationService = telegramNotificationService;
        _mediator = mediator;
    }

    public bool CanHandle(Update update)
    {
        return update.Message?.Text is BotConstants.StartCommand;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is { Chat.Id: long tgUserId, MessageId: int messageId })
        {
            var result = await _mediator.Send(new StartMessage(tgUserId, messageId), cancellationToken);

            await _telegramNotificationService.SendMessageAsync(tgUserId, result.GetDisplayText(), result.ToTelegramMarkup(), cancellationToken);
        }
    }
}
