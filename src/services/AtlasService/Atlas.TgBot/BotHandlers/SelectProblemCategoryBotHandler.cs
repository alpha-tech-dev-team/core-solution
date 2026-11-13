using Atlas.Application.Common;
using Atlas.Application.Features.IncidentCategories;
using Atlas.Domain.Enums;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.Extensions;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Atlas.TgBot.BotHandlers;

public sealed class SelectProblemCategoryBotHandler : IBotHandler
{
    private static readonly InlineKeyboardMarkup EmptyMarkup = new(Array.Empty<InlineKeyboardButton>());

    private readonly IMediator _mediator;
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IUserErrorMapper _userErrorMapper;

    public SelectProblemCategoryBotHandler(
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
        return update.CallbackQuery?.Data?.StartsWith(CallbackConstants.CategoryPrefix, StringComparison.Ordinal) is true;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is not { Data: string data, Message: { Chat.Id: long telegramUserId, MessageId: int messageId } })
        {
            return;
        }

        var categoryId = data.ParseCategoryId();

        var result = categoryId == Guid.Empty
            ? Result<WorkflowResponse>.Failure(nameof(ErrorCode.InvalidCategoryId))
            : await _mediator.Send(
                new SelectProblemCategoryMessage(telegramUserId, messageId, categoryId),
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
