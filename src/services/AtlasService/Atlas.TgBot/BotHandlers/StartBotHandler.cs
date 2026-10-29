using Atlas.Core.Constants.Localization;
using Atlas.Core.Enums;
using Atlas.Core.Interfaces;
using Atlas.TgBot.Constants;
using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using CoreSolution.Shared.Models;
using Telegram.Bot.Types;
using CoreSolution.Shared.Results;

namespace Atlas.TgBot.BotHandlers;

public sealed class StartBotHandler : IBotHandler
{
    private readonly ISessionService _sessionService;
    private readonly ITelegramNotificationService _telegram;
    private readonly ILocalizer _localizer;
    private readonly ILogger<StartBotHandler> _logger;

    public StartBotHandler(
        ISessionService sessionService,
        ITelegramNotificationService telegram,
        ILocalizer localizer,
        ILogger<StartBotHandler> logger)
    {
        _sessionService = sessionService;
        _telegram = telegram;
        _localizer = localizer;
        _logger = logger;
    }

    public bool CanHandle(Update update)
    {
        return update.Message?.Text == StartConstants.Command;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        var message = update.Message!;
        var tgUserId = message.From!.Id;

        _logger.LogInformation("Вызван /start от {TgUserId}", tgUserId);

        var session = await _sessionService.GetSession(tgUserId, cancellationToken);

        TelegramMessageContent content;

        if (session is null)
        {
            _logger.LogInformation("Сессия не найдена, требуется телефон пользователя {TgUserId}", tgUserId);

            content = new TelegramMessageContent
            {
                Text = _localizer.GetPhrase(WorkflowStep.Idle, LocalizationKeysConstants.Idle.PhoneRequest),
                RequestContact = StartConstants.DefaultRequestContact
            };
        }
        else
        {
            _logger.LogInformation("Сессия пользователя {TgUserId} найдена", tgUserId);

            content = CreateMenuContent();
        }

        var result = Result<TelegramMessageContent>.Success(content);

        await _telegram.SendMessageAsync(
            message.Chat.Id,
            result.GetDisplayText(),
            result.ToTelegramMarkup(),
            cancellationToken: cancellationToken);   
        }

    private TelegramMessageContent CreateMenuContent()
    {
        return new TelegramMessageContent
        {
            Text = _localizer.GetPhrase(WorkflowStep.ProblemCategorySelection, LocalizationKeysConstants.ProblemCategorySelection.Text),                ButtonRows =
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
