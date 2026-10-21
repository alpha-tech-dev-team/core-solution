using Building.Telegram.Abstractions;
using MediatR;
using Pandora.App.Features.Credentials;
using Pandora.TgBot.Constants;
using Telegram.Bot.Types;

namespace Pandora.TgBot.Handlers;

public sealed class ServiceCallbackHandler : IBotHandler
{
    private readonly ITelegramNotificationService _telegram;
    private readonly IMediator _mediator;

    public ServiceCallbackHandler(ITelegramNotificationService telegram, IMediator mediator)
    {
        _telegram = telegram;
        _mediator = mediator;
    }

    public bool CanHandle(Update update)
    {
        return update.CallbackQuery?.Data?.StartsWith(BotConstants.ServiceCallbackPrefix, StringComparison.Ordinal) is true;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is { Data: string data, Message.Chat.Id: long chatId })
        {
            var serviceName = data[BotConstants.ServiceCallbackPrefix.Length..];
            var response = await _mediator.Send(new GetCredentialsMessage(chatId, serviceName), cancellationToken);
            await _telegram.SendMessageAsync(chatId, response.Text, null, cancellationToken);
        }
    }
}
