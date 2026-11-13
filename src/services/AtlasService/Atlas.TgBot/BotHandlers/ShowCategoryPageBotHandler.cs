using Atlas.Application.Common;
using Atlas.Application.Features.Menu;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.Extensions;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Atlas.TgBot.BotHandlers;

public sealed class ShowCategoryPageBotHandler : IBotHandler
{
    private static readonly InlineKeyboardMarkup EmptyMarkup = new(Array.Empty<InlineKeyboardButton>());

    private readonly IMediator _mediator;
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IUserErrorMapper _userErrorMapper;

    public ShowCategoryPageBotHandler(
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
        return update.CallbackQuery?.Data?.StartsWith(CallbackConstants.CategoryPagePrefix, StringComparison.Ordinal) is true;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is not { Data: string data, Message: { Chat.Id: long telegramUserId, MessageId: int messageId } })
        {
            return;
        }

        var result = await _mediator.Send(
            new ShowMenuMessage(telegramUserId, messageId, data.ParsePage()),
            cancellationToken);

        var markup = result.ToTelegramMarkup() as InlineKeyboardMarkup ?? EmptyMarkup;

        await _telegramNotificationService.SendOrEditAsync(
            telegramUserId,
            messageId,
            result.GetUserText(_userErrorMapper),
            markup,
            cancellationToken);
    }
}
