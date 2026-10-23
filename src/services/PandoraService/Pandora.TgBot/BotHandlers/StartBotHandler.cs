using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using MediatR;
using Pandora.App.Features.Start;
using Pandora.Core.Interfaces;
using Pandora.TgBot.Constants;
using Telegram.Bot.Types;

namespace Pandora.TgBot.BotHandlers;

public sealed class StartBotHandler : IBotHandler
{
    private readonly ITelegramNotificationService _telegram;
    private readonly IMediator _mediator;
    private readonly ILocalizer _localizer;

    public StartBotHandler(ITelegramNotificationService telegram, IMediator mediator, ILocalizer localizer)
    {
        _telegram = telegram;
        _mediator = mediator;
        _localizer = localizer;
    }

    public bool CanHandle(Update update)
    {
        return update.Message?.Text is BotConstants.StartCommand;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is { Chat.Id: long telegramUserId, MessageId: int messageId })
        {
            var result = await _mediator.Send(new StartMessage(telegramUserId, messageId), cancellationToken);

            await _telegram.SendMessageAsync(telegramUserId, result.GetDisplayText(_localizer), result.ToTelegramMarkup(), cancellationToken);
        }
    }
}
