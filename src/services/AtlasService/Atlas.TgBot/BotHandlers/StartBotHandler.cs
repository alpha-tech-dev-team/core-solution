using Atlas.Core.Constants.Localization;
using Atlas.Core.Enums;
using Atlas.Core.Interfaces;
using Atlas.TgBot.Constants;
using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using Telegram.Bot.Types;

namespace Atlas.TgBot.BotHandlers;

public sealed class StartBotHandler : IBotHandler
{
    private readonly ISessionService _sessionService;
    private readonly ITelegramNotificationService _telegram;
    private readonly ILocalizer _localizer;
    private readonly ILogger<StartBotHandler> _logger;

    public StartBotHandler(ISessionService sessionService, ITelegramNotificationService telegram, ILocalizer localizer, ILogger<StartBotHandler> logger)
    {
        _sessionService = sessionService;
        _telegram = telegram;
        _localizer = localizer;
        _logger = logger;
    }

    public bool CanHandle(Update update) => update.Message?.Text == StartConstants.Command;

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        var message = update.Message!;
        var telegramUserId = message.From!.Id;
        _logger.LogInformation("Вызван /start от {TgUserId}", telegramUserId);
        await HandleStartAsync(message.Chat.Id, telegramUserId, cancellationToken);
    }

    public async Task HandleStartAsync(long chatId, long telegramUserId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Запрос номера телефона для пользователя {TgUserId}", telegramUserId);

        var content = new TelegramMessageContent
        {
            Text = _localizer.GetPhrase(
                WorkflowStep.Idle,
                LocalizationKeysConstants.Idle.PhoneRequest,
                LocalizationConstants.DefaultLanguage),
            RequestContact = StartConstants.DefaultRequestContact
        };

        var result = Result<TelegramMessageContent>.Success(content);
        await _telegram.SendMessageAsync(
            chatId,
            result.GetDisplayText(),
            result.ToTelegramMarkup(),
            cancellationToken: cancellationToken);
    }

    public async Task SendWorkflowMenuAsync(
        long chatId,
        long telegramUserId,
        CancellationToken cancellationToken)
    {
        var session = await _sessionService.GetSession(telegramUserId, cancellationToken);
        var language = session?.Language ?? LocalizationConstants.DefaultLanguage;
        var result = Result<TelegramMessageContent>.Success(CreateMenuContent(language));

        await _telegram.SendMessageAsync(
            chatId,
            result.GetDisplayText(),
            result.ToTelegramMarkup(),
            cancellationToken: cancellationToken);
    }

    private TelegramMessageContent CreateMenuContent(Language language)
    {
        return new TelegramMessageContent
        {
            Text = _localizer.GetPhrase(WorkflowStep.ProblemCategorySelection, LocalizationKeysConstants.ProblemCategorySelection.Text, language),
            ButtonRows =
            [
                new TelegramButtonRow(
                    new TelegramButton(_localizer.GetPhrase(WorkflowStep.ProblemCategorySelection, LocalizationKeysConstants.ProblemCategorySelection.Problems, language), StartConstants.MenuProblems),
                    new TelegramButton(_localizer.GetPhrase(WorkflowStep.ProblemCategorySelection, LocalizationKeysConstants.ProblemCategorySelection.Profile, language), StartConstants.MenuProfile))
            ]
        };
    }
}
