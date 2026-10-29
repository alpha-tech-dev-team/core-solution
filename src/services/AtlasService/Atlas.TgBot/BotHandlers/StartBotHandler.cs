using Atlas.App.Features.Start;
using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace Atlas.TgBot.BotHandlers;

public sealed class StartBotHandler : IBotHandler
{
    private readonly ITelegramBotClient _bot;
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
        return update.Message?.Text == StartConstants.Command;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        var message = update.Message!;
        var tgUserId = message.From!.Id;

        _logger.LogInformation($"Вызван /start от {tgUserId}");

        var result = await _mediator.Send(new StartRequest(tgUserId), cancellationToken);
        await _telegram.SendMessageAsync(
            message.Chat.Id, 
            result.GetDisplayText(), 
            result.ToTelegramMarkup(), 
            cancellationToken: cancellationToken);    
        }
}