using Atlas.App.Features.Activation;
using Atlas.Core.Constants.Localization;
using Atlas.Core.Enums;
using Atlas.Core.Interfaces;
using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using MediatR;
using Telegram.Bot.Types;

namespace Atlas.TgBot.BotHandlers;

public sealed class ContactBotHandler : IBotHandler
{
    public const string Command = "/start";
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IMediator _mediator;
    private readonly ILocalizer _localizer;

    public ContactBotHandler(
        ITelegramNotificationService telegramNotificationService,
        IMediator mediator,
        ILocalizer localizer)
    {
        _telegramNotificationService = telegramNotificationService;
        _mediator = mediator;
        _localizer = localizer;
    }

    public bool CanHandle(Update update)
    {
        return update.Message?.Contact is not null || update.Message?.Text is Command;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { Chat.Id: long tgUserId, MessageId: int messageId })
        {
            return;
        }

        var phone = update.Message.Contact?.PhoneNumber;

        var result = await _mediator.Send(new SubmitPhoneMessage(tgUserId, messageId, phone), cancellationToken);

        await _telegramNotificationService.SendMessageAsync(tgUserId, result.GetDisplayText(), result.ToTelegramMarkup(), cancellationToken);
        }
}
