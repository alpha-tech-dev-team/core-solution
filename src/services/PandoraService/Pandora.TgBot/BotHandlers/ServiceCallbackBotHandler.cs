using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.Extensions;
using MediatR;
using Pandora.TgBot.Application.Common;
using Pandora.TgBot.Application.Features.Credentials;
using Pandora.TgBot.Credentials;
using Telegram.Bot.Types;

namespace Pandora.TgBot.BotHandlers;

public sealed class ServiceCallbackBotHandler : IBotHandler
{
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IMediator _mediator;
    private readonly IUserErrorMapper _userErrorMapper;
    private readonly CredentialMessageScheduler _credentialMessageScheduler;

    public ServiceCallbackBotHandler(
        ITelegramNotificationService telegramNotificationService,
        IMediator mediator,
        IUserErrorMapper userErrorMapper,
        CredentialMessageScheduler credentialMessageScheduler)
    {
        _telegramNotificationService = telegramNotificationService;
        _mediator = mediator;
        _userErrorMapper = userErrorMapper;
        _credentialMessageScheduler = credentialMessageScheduler;
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
            var sentMessage = await _telegramNotificationService.SendMessageAsync(
                tgUserId,
                result.GetUserText(_userErrorMapper),
                result.ToTelegramMarkup(),
                cancellationToken,
                result.ToTelegramSendOptions());

            if (result.IsSuccess && result.Value is { ContainsSensitiveData: true })
            {
                await _credentialMessageScheduler.ScheduleAsync(
                    tgUserId,
                    sentMessage.MessageId,
                    itemId,
                    cancellationToken);
            }
        }
    }
}
