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
    private readonly ITelegramNotificationService _telegram;
    private readonly IMediator _mediator;
    private readonly ILocalizer _localizer;

    public const string Command = "/start";

    public ContactBotHandler(
        ITelegramNotificationService telegram,
        IMediator mediator,
        ILocalizer localizer)
    {
        _telegram = telegram;
        _mediator = mediator;
        _localizer = localizer;
    }

    public bool CanHandle(Update update)
    {
        return update.Message?.Contact is not null || update.Message?.Text is Command;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
        {
            return;
        }

        var chatId = message.Chat.Id;

        var result = await _mediator.Send(
            new SubmitPhoneMessage(chatId, message.MessageId, message.Contact?.PhoneNumber),
            cancellationToken);

        var content = result.Value;
        var markup = content is { RequestContact: true }
            ? content.ToReplyMarkup(
                _localizer.GetPhrase(WorkflowStep.Idle, LocalizationKeysConstants.Idle.PhoneRequest))
            : result.ToTelegramMarkup();

        await _telegram.SendMessageAsync(
            chatId,
            result.GetDisplayText(),
            markup,
            cancellationToken);
    }
}
