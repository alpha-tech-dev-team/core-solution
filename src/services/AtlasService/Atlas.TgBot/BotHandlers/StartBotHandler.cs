using Atlas.App.Features.Start;
using Atlas.TgBot.Constants;
using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using MediatR;
using Telegram.Bot.Types;

namespace Atlas.TgBot.BotHandlers;

public sealed class StartBotHandler : IBotHandler
{
    private readonly IMediator _mediator;
    private readonly ITelegramNotificationService _telegram;

    public StartBotHandler(IMediator mediator, ITelegramNotificationService telegram)
    {
        _mediator = mediator;
        _telegram = telegram;
    }

    public bool CanHandle(Update update)
    {
        return update.Message?.Text is StartConstants.Command;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        var message = update.Message!;
        
        var request = new StartRequest(message.Chat.Id);

        var answer = await _mediator.Send(request, cancellationToken);

        await _telegram.SendMessageAsync(message.Chat.Id, answer.GetTextOrErrors(), answer.ToTelegramMarkup(), cancellationToken: cancellationToken);
    }
}
