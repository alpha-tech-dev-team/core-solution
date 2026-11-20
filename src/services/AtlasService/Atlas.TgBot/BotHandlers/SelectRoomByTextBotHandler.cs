using Atlas.Application.Common;
using Atlas.Application.Features.Rooms;
using Atlas.Domain.Enums;
using Atlas.Domain.Models;
using Atlas.TgBot.Constants;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.Extensions;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Atlas.TgBot.BotHandlers;

public sealed class SelectRoomByTextBotHandler : IBotHandler
{
    private readonly IMediator _mediator;
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IUserErrorMapper _userErrorMapper;
    private readonly IRequestContext _requestContext;
    private readonly ISessionService _sessionService;

    public SelectRoomByTextBotHandler(
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
        return update.Message?.Text is { } text
            && !text.StartsWith(TelegramCommandConstants.Prefix, StringComparison.Ordinal)
            && _requestContext.Session is AtlasUserSession { State: nameof(WorkflowStep.RoomSelection) };
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is { Text: string text, Chat.Id: long telegramUserId, MessageId: int messageId })
        {
            var menuMessageId = (_requestContext.Session as AtlasUserSession)?.RoomMenuMessageId;

            var result = await _mediator.Send(
                new SelectRoomByTextMessage(telegramUserId, messageId, text, menuMessageId),
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
