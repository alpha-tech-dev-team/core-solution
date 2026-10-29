using Atlas.App.Features.Activation;
using Atlas.Core.Constants.Localization;
using Atlas.Core.Enums;
using Atlas.Core.Interfaces;
using Atlas.TgBot.Constants;
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
        return update.Message?.Contact is not null;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is { Chat.Id: long tgUserId, MessageId: int messageId, Contact.PhoneNumber: string phone })
        {
            var result = await _mediator.Send(new SubmitPhoneMessage(tgUserId, messageId, phone), cancellationToken);

            if (result.IsSuccess)
            {
                var menuResult = Result<TelegramMessageContent>.Success(CreateMenuContent());

                await _telegramNotificationService.SendMessageAsync(
                    tgUserId,
                    menuResult.GetDisplayText(),
                    menuResult.ToTelegramMarkup(),
                    cancellationToken);
            }
            else
            {
                await _telegramNotificationService.SendMessageAsync(
                    tgUserId,
                    result.GetDisplayText(),
                    result.ToTelegramMarkup(),
                    cancellationToken);
            }
        }
    }

    private TelegramMessageContent CreateMenuContent()
    {
        return new TelegramMessageContent
        {
            Text = _localizer.GetPhrase(WorkflowStep.ProblemCategorySelection, LocalizationKeysConstants.ProblemCategorySelection.Text),
            ButtonRows =
            [
                new TelegramButtonRow(
                    new TelegramButton(
                        _localizer.GetPhrase(WorkflowStep.ProblemCategorySelection, LocalizationKeysConstants.ProblemCategorySelection.Problems),
                        StartConstants.MenuProblems),
                    new TelegramButton(
                        _localizer.GetPhrase(WorkflowStep.ProblemCategorySelection, LocalizationKeysConstants.ProblemCategorySelection.Profile),
                        StartConstants.MenuProfile))
            ]
        };
    }
}
