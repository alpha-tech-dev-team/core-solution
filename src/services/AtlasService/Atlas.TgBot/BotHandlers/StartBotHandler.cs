using Atlas.App.Features.Start;
using Atlas.TgBot.Constants;
using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Atlas.TgBot.BotHandlers;

public sealed class StartBotHandler : IBotHandler
{
    private readonly IMediator _mediator;
    private readonly ITelegramNotificationService _telegram;
    private readonly ILogger<StartBotHandler> _logger;

    public StartBotHandler(IMediator mediator, ITelegramNotificationService telegram, ILogger<StartBotHandler> logger)
    {
        _mediator = mediator;
        _telegram = telegram;
        _logger = logger;
    }

    public bool CanHandle(Update update)
    {
        return update.Message?.Text is StartConstants.Command;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        var message = update.Message!;
        var incomingChatId = message.Chat.Id;
        
        _logger.LogInformation("Incoming /start from chat_id: {ChatId}", incomingChatId);
        
        var request = new StartRequest(incomingChatId);

        var answer = await _mediator.Send(request, cancellationToken);

        await _telegram.SendMessageAsync(message.Chat.Id, answer.GetTextOrErrors(), answer.ToTelegramMarkup(), cancellationToken: cancellationToken);
    }
}
