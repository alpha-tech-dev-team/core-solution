using Atlas.Application.Common;
using Atlas.Application.Features.Rooms;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.Extensions;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Atlas.TgBot.BotHandlers;

public sealed class ShowRoomPageBotHandler : IBotHandler
{
    private readonly IMediator _mediator;
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IUserErrorMapper _userErrorMapper;

    public ShowRoomPageBotHandler(
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
        return update.CallbackQuery?.Data?.StartsWith(CallbackConstants.RoomPagePrefix, StringComparison.Ordinal) is true;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is { Data: string data, Message: { Chat.Id: long telegramUserId, MessageId: int messageId } })
        {
            var result = await _mediator.Send(
                new ShowRoomsMessage(telegramUserId, messageId, data.ParseRoomPage()),
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
