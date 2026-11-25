using Atlas.Application.Common;
using Atlas.Application.Features.Menu;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.Extensions;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Atlas.TgBot.BotHandlers;

public sealed class ShowCategoryPageBotHandler : IBotHandler
{
    private readonly IMediator _mediator;
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IUserErrorMapper _userErrorMapper;
    private readonly IRequestContext _requestContext;
    private readonly ISessionService _sessionService;

    public ShowCategoryPageBotHandler(
        IMediator mediator,
        ITelegramNotificationService telegramNotificationService,
        IUserErrorMapper userErrorMapper,
        IRequestContext requestContext,
        ISessionService sessionService)
    {
        _mediator = mediator;
        _telegramNotificationService = telegramNotificationService;
        _userErrorMapper = userErrorMapper;
        _requestContext = requestContext;
        _sessionService = sessionService;
    }

    public bool CanHandle(Update update)
    {
        return update.CallbackQuery?.Data?.StartsWith(CallbackConstants.CategoryPagePrefix, StringComparison.Ordinal) is true;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is { Data: string data, Message: { Chat.Id: long telegramUserId, MessageId: int messageId } })
        {
            var result = await _mediator.Send(
                new ShowMenuMessage(telegramUserId, messageId, data.ParsePage()),
                cancellationToken);

            await _telegramNotificationService.SendReplyAsync(
                _requestContext,
                _sessionService,
                telegramUserId,
                result.GetUserText(_userErrorMapper),
                result.ToTelegramMarkup() as InlineKeyboardMarkup,
                cancellationToken);
        }
    }
}
