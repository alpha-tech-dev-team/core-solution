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
    private readonly IRequestContext _requestContext;

    public SelectRoomByTextBotHandler(
        IMediator mediator,
        ITelegramNotificationService telegramNotificationService,
        IUserErrorMapper userErrorMapper,
        IRequestContext requestContext)
    {
        _mediator = mediator;
        _telegramNotificationService = telegramNotificationService;
        _userErrorMapper = userErrorMapper;
        _requestContext = requestContext;
    }

    public bool CanHandle(Update update)
    {
        return update.Message?.Text is not null
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

            if (menuMessageId is null)
            {
                await _telegramNotificationService.SendMessageAsync(
                    telegramUserId,
                    result.GetUserText(_userErrorMapper),
                    result.ToTelegramMarkup() as InlineKeyboardMarkup,
                    cancellationToken);

                return;
            }

            await _telegramNotificationService.SendOrEditAsync(
                telegramUserId,
                menuMessageId.Value,
                result.GetUserText(_userErrorMapper),
                result.ToTelegramMarkup() as InlineKeyboardMarkup,
                cancellationToken);
        }
    }
}
