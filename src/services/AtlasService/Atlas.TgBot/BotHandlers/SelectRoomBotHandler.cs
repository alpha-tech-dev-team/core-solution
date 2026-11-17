using Atlas.Application.Common;
using Atlas.Application.Features.Rooms;
using Atlas.Domain.Enums;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.Extensions;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Atlas.TgBot.BotHandlers;

public sealed class SelectRoomBotHandler : IBotHandler
{
    private readonly IMediator _mediator;
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IUserErrorMapper _userErrorMapper;

    public SelectRoomBotHandler(
        IMediator mediator,
        ITelegramNotificationService telegramNotificationService,
        IUserErrorMapper userErrorMapper)
    {
        _mediator = mediator;
        _telegramNotificationService = telegramNotificationService;
        _userErrorMapper = userErrorMapper;
    }

    public bool CanHandle(Update update)
    {
        return update.CallbackQuery?.Data?.StartsWith(CallbackConstants.RoomPrefix, StringComparison.Ordinal) is true;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is { Data: string data, Message: { Chat.Id: long telegramUserId, MessageId: int messageId } })
        {
            var roomId = data.ParseRoomId();

            var result = roomId == Guid.Empty
                ? Result<WorkflowResponse>.Failure(nameof(ErrorCode.InvalidRoomId))
                : await _mediator.Send(
                    new SelectRoomMessage(telegramUserId, messageId, roomId),
                    cancellationToken);

            await _telegramNotificationService.SendOrEditAsync(
                telegramUserId,
                messageId,
                result.GetUserText(_userErrorMapper),
                result.ToTelegramMarkup() as InlineKeyboardMarkup,
                cancellationToken);
        }
    }
}
