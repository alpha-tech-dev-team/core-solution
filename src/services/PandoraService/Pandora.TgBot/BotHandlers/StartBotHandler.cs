using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using MediatR;
using Pandora.App.Features.Start;
using Pandora.TgBot.Constants;
using Telegram.Bot.Types;

namespace Pandora.TgBot.BotHandlers;

public sealed class StartBotHandler : IBotHandler
{
    private readonly ITelegramNotificationService _telegram;
    private readonly IMediator _mediator;

    public StartBotHandler(ITelegramNotificationService telegram, IMediator mediator)
    {
        _telegram = telegram;
        _mediator = mediator;
    }

    public bool CanHandle(Update update)
    {
        return update.Message?.Text is BotConstants.StartCommand;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is { Chat.Id: long chatId })
        {
            var result = await _mediator.Send(new StartMessage(chatId), cancellationToken);

            await _telegram.SendMessageAsync(chatId, result.GetDisplayText(), result.ToTelegramMarkup(), cancellationToken);
        }
    }
}
