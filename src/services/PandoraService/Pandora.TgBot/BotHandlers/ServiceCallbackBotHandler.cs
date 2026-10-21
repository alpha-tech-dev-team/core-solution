using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using MediatR;
using Pandora.App.Common;
using Pandora.App.Features.Credentials;
using Telegram.Bot.Types;

namespace Pandora.TgBot.BotHandlers;

public sealed class ServiceCallbackBotHandler : IBotHandler
{
    private readonly ITelegramNotificationService _telegram;
    private readonly IMediator _mediator;

    public ServiceCallbackBotHandler(ITelegramNotificationService telegram, IMediator mediator)
    {
        _telegram = telegram;
        _mediator = mediator;
    }

    public bool CanHandle(Update update)
    {
        return update.CallbackQuery?.Data?.StartsWith(CallbackConstants.ServicePrefix, StringComparison.Ordinal) is true;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is { Data: string data, Message.Chat.Id: long chatId })
        {
            var serviceName = data[CallbackConstants.ServicePrefix.Length..];
            var result = await _mediator.Send(new GetCredentialsMessage(chatId, serviceName), cancellationToken);

            await _telegram.SendMessageAsync(chatId, result.GetTextOrErrors(), result.ToTelegramMarkup(), cancellationToken);
        }
    }
}
