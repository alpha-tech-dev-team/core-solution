using Atlas.App.Features.Text;
using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using MediatR;
using Telegram.Bot.Types;

namespace Atlas.TgBot.BotHandlers;

public sealed class TextBotHandler : IBotHandler
{
    private readonly IMediator _mediator;
    private readonly ITelegramNotificationService _telegram;
    private readonly string _unnamedUser = "Пользователь";


    public TextBotHandler(IMediator mediator, ITelegramNotificationService telegram)
    {
        _mediator = mediator;
        _telegram = telegram;
    }

    public bool CanHandle(Update update)
    {

        return update.Message?.Text is not null;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        var message = update.Message!;
        var command = new TextCommand(
            message.Chat.Id,
            message.Text!,
            message.From?.FirstName ?? _unnamedUser);

        var answer = await _mediator.Send(command, cancellationToken);

        await _telegram.SendMessageAsync(message.Chat.Id, answer.GetTextOrErrors(), cancellationToken: cancellationToken);
    }
}
