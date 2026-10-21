using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using Building.Telegram.Models;
using MediatR;
using Pandora.App.Features.Start;
using Pandora.TgBot.Constants;
using Telegram.Bot.Types;

namespace Pandora.TgBot.Handlers;

public sealed class StartCommandHandler : IBotHandler
{
    private readonly ITelegramNotificationService _telegram;
    private readonly IMediator _mediator;

    public StartCommandHandler(ITelegramNotificationService telegram, IMediator mediator)
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
            var response = await _mediator.Send(new StartMessage(chatId), cancellationToken);
            var replyMarkup = response.Services
                .Select(service => new TelegramButtonRow(new TelegramButton(service, service)))
                .ToInlineKeyboardMarkup();

            await _telegram.SendMessageAsync(chatId, response.Text, replyMarkup, cancellationToken);
        }
    }
}
