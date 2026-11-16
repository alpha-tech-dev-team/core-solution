using Atlas.Application.Common;
using Atlas.Application.Features.Rooms;
using Atlas.Domain.Enums;
using Atlas.Domain.Models;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
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
    private readonly ISessionService _sessionService;

    public SelectRoomByTextBotHandler(
        IMediator mediator,
        ITelegramNotificationService telegramNotificationService,
        IUserErrorMapper userErrorMapper,
        ISessionService sessionService)
    {
        _mediator = mediator;
        _telegramNotificationService = telegramNotificationService;
        _userErrorMapper = userErrorMapper;
        _sessionService = sessionService;
    }

    public bool CanHandle(Update update)
    {
        if (update.Message?.Text is null)
        {
            return false;
        }

        var session = GetSession(update.Message.Chat.Id);

        return session is AtlasUserSession atlasSession
            && atlasSession.State == nameof(WorkflowStep.RoomSelection);
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { Text: string text, Chat.Id: long telegramUserId, MessageId: int messageId })
        {
            return;
        }

        var session = GetSession(telegramUserId);
        var menuMessageId = session?.RoomMenuMessageId;

        var result = await _mediator.Send(
            new SelectRoomByTextMessage(telegramUserId, messageId, text, menuMessageId),
            cancellationToken);

        var markup = result.ToTelegramMarkup() as InlineKeyboardMarkup;
        var responseText = result.GetUserText(_userErrorMapper);

        if (menuMessageId is null)
        {
            await _telegramNotificationService.SendMessageAsync(telegramUserId, responseText, markup, cancellationToken);
            return;
        }

        await _telegramNotificationService.SendOrEditAsync(
            telegramUserId,
            menuMessageId.Value,
            responseText,
            markup,
            cancellationToken);
    }

    private AtlasUserSession? GetSession(long telegramUserId)
    {
        var session = _sessionService
            .GetSession(telegramUserId, CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        return session as AtlasUserSession;
    }
}
