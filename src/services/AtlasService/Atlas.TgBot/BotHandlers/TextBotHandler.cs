using Atlas.App.Features.Text;
using Atlas.TgBot.Constants;
using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Atlas.TgBot.BotHandlers;

public sealed class TextBotHandler : IBotHandler
{
    private readonly IMediator _mediator;
    private readonly ITelegramNotificationService _telegram;
    private readonly ILogger<TextBotHandler> _logger;

    public TextBotHandler(IMediator mediator, ITelegramNotificationService telegram, ILogger<TextBotHandler> logger)
    {
        _mediator = mediator;
        _telegram = telegram;
        _logger = logger;
    }

    public bool CanHandle(Update update)
    {
        return update.Message?.Text is not null && update.Message.Text != StartConstants.Command;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        var message = update.Message!;
        var tgUserId = message.From!.Id;

        _logger.LogInformation("Incoming text message from TgUserId: {TgUserId}", tgUserId);

        var request = new TextRequest(
            message.Chat.Id,
            message.Text!,
            message.From?.FirstName ?? BotConstants.UnnamedUser);

        var answer = await _mediator.Send(request, cancellationToken);

        await _telegram.SendMessageAsync(message.Chat.Id, answer.GetTextOrErrors(), answer.ToTelegramMarkup(), cancellationToken: cancellationToken);
    }
}