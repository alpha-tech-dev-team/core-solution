using Atlas.Application.Features.Menu;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using Atlas.TgBot.Options;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.Extensions;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace Atlas.TgBot.BotHandlers;

public sealed class StartBotHandler : IBotHandler
{
    private const string StartCommand = "/start";

    private readonly IMediator _mediator;
    private readonly ISessionService _sessionService;
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IUserErrorMapper _userErrorMapper;
    private readonly PhoneConfirmationOptions _options;
    private readonly IHostEnvironment _environment;

    public StartBotHandler(
        IMediator mediator,
        ISessionService sessionService,
        ITelegramNotificationService telegramNotificationService,
        IUserErrorMapper userErrorMapper,
        IOptions<PhoneConfirmationOptions> options,
        IHostEnvironment environment)
    {
        _mediator = mediator;
        _sessionService = sessionService;
        _telegramNotificationService = telegramNotificationService;
        _userErrorMapper = userErrorMapper;
        _options = options.Value;
        _environment = environment;
    }

    public bool CanHandle(Update update)
    {
        return IsSkipEnabled() && update.Message?.Text is StartCommand;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { Chat.Id: long telegramUserId, MessageId: int messageId })
        {
            return;
        }

        await _sessionService.SaveSession(
            UserSession.Create(telegramUserId, nameof(WorkflowStep.ProblemCategorySelection)),
            cancellationToken);

        var result = await _mediator.Send(
            new ShowMenuMessage(telegramUserId, messageId, PaginationLimits.FirstPage),
            cancellationToken);

        await _telegramNotificationService.SendMessageAsync(
            telegramUserId,
            result.GetUserText(_userErrorMapper),
            result.ToTelegramMarkup(),
            cancellationToken);
    }

    private bool IsSkipEnabled()
    {
        return !_options.Enabled && !_environment.IsProduction();
    }
}
