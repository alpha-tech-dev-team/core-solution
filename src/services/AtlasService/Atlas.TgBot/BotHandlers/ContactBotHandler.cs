
using Atlas.App.Features.Activation;
using Atlas.Core.Constants.Localization;
using Atlas.Core.Enums;
using Atlas.Core.Interfaces;
using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Telegram.Bot.Types;

namespace Atlas.TgBot.BotHandlers;

public sealed class ContactBotHandler : IBotHandler
{
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IMediator _mediator;
    private readonly StartBotHandler _startBotHandler;
    private readonly ILocalizer _localizer;

    public ContactBotHandler(
        ITelegramNotificationService telegramNotificationService,
        IMediator mediator,
        StartBotHandler startBotHandler,
        ILocalizer localizer)
    {
        _telegramNotificationService = telegramNotificationService;
        _mediator = mediator;
        _startBotHandler = startBotHandler;
        _localizer = localizer;
    }

    public bool CanHandle(Update update) =>
        update.Message?.Contact is not null;

    public async Task HandleAsync(
        Update update,
        CancellationToken cancellationToken)
    {
        if (update.Message is not
            {
                Chat.Id: long tgUserId,
                MessageId: int messageId,
                Contact.PhoneNumber: string phone
            })
        {
            return;
        }

        var result = await _mediator.Send(
            new SubmitPhoneMessage(tgUserId, messageId, phone),
            cancellationToken);

        if (result.IsSuccess)
        {
            await _startBotHandler.SendWorkflowMenuAsync(
                tgUserId,
                tgUserId,
                cancellationToken);

            return;
        }

        var content = new TelegramMessageContent
        {
            Text = _localizer.GetPhrase(
                WorkflowStep.Idle,
                LocalizationKeysConstants.Idle.AccessNotConfirmed,
                LocalizationConstants.DefaultLanguage)
        };

        var messageResult = Result<TelegramMessageContent>.Success(content);

        await _telegramNotificationService.SendMessageAsync(
            tgUserId,
            messageResult.GetDisplayText(),
            messageResult.ToTelegramMarkup(),
            cancellationToken);
    }
}
