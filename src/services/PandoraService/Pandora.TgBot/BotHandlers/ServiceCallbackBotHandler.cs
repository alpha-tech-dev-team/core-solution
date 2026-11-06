using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.Extensions;
using MediatR;
using Pandora.TgBot.Common;
using Pandora.TgBot.Features.Credentials;
using Telegram.Bot.Types;

namespace Pandora.TgBot.BotHandlers;

public sealed class ServiceCallbackBotHandler : IBotHandler
{
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IMediator _mediator;
    private readonly IUserErrorMapper _userErrorMapper;

    public ServiceCallbackBotHandler(ITelegramNotificationService telegramNotificationService, IMediator mediator, IUserErrorMapper userErrorMapper)
    {
        _telegramNotificationService = telegramNotificationService;
        _mediator = mediator;
        _userErrorMapper = userErrorMapper;
    }

    public bool CanHandle(Update update)
    {
        return update.CallbackQuery?.Data?.StartsWith(CallbackConstants.ServicePrefix, StringComparison.Ordinal) is true;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is { Data: string data, Message: { Chat.Id: long tgUserId, MessageId: int messageId } })
        {
            var itemId = data[CallbackConstants.ServicePrefix.Length..];
            var result = await _mediator.Send(new GetCredentialsMessage(tgUserId, messageId, itemId), cancellationToken);

            await _telegramNotificationService.SendMessageAsync(tgUserId, result.GetUserText(_userErrorMapper), result.ToTelegramMarkup(), cancellationToken);
        }
    }
}
