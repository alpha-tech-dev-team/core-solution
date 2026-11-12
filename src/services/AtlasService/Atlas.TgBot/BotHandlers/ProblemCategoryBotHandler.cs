using Atlas.Application.Common;
using Atlas.Application.Features.IncidentCategories;
using Atlas.Application.Features.Menu;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.Extensions;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Atlas.TgBot.BotHandlers;

public sealed class ProblemCategoryBotHandler : IBotHandler
{
    private static readonly InlineKeyboardMarkup EmptyMarkup = new(Array.Empty<InlineKeyboardButton>());

    private readonly IMediator _mediator;
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IUserErrorMapper _userErrorMapper;

    public ProblemCategoryBotHandler(
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
        var data = update.CallbackQuery?.Data;

        return data is not null
            && (data.StartsWith(CallbackConstants.CategoryPrefix, StringComparison.Ordinal)
                || data.StartsWith(CallbackConstants.CategoryPagePrefix, StringComparison.Ordinal));
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery is not { Data: string data, Message: { Chat.Id: long telegramUserId, MessageId: int messageId } })
        {
            return;
        }

        var result = data.StartsWith(CallbackConstants.CategoryPagePrefix, StringComparison.Ordinal)
            ? await _mediator.Send(
                new ShowMenuMessage(telegramUserId, messageId, ParsePage(data)),
                cancellationToken)
            : await SelectCategoryAsync(telegramUserId, messageId, data, cancellationToken);

        var markup = result.ToTelegramMarkup() as InlineKeyboardMarkup ?? EmptyMarkup;

        await _telegramNotificationService.SendOrEditAsync(
            telegramUserId,
            messageId,
            result.GetUserText(_userErrorMapper),
            markup,
            cancellationToken);
    }

    private async Task<Result<WorkflowResponse>> SelectCategoryAsync(
        long telegramUserId,
        int messageId,
        string data,
        CancellationToken cancellationToken)
    {
        var categoryId = ParseCategoryId(data);

        if (categoryId == Guid.Empty)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentCategoryNotFound));
        }

        return await _mediator.Send(
            new SelectProblemCategoryMessage(telegramUserId, messageId, categoryId),
            cancellationToken);
    }

    private static int ParsePage(string data)
    {
        var value = data[CallbackConstants.CategoryPagePrefix.Length..];

        return int.TryParse(value, out var page) ? page : PaginationLimits.FirstPage;
    }

    private static Guid ParseCategoryId(string data)
    {
        var value = data[CallbackConstants.CategoryPrefix.Length..];

        return Guid.TryParse(value, out var categoryId) ? categoryId : Guid.Empty;
    }
}
