using Atlas.App.Features.Start;
using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Atlas.TgBot.BotHandlers;

public sealed class StartBotHandler : IBotHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly IMediator _mediator;
    private readonly ITelegramNotificationService _telegram;
    private readonly ILogger<StartBotHandler> _logger;

    public StartBotHandler(IMediator mediator, ITelegramNotificationService telegram, ILogger<StartBotHandler> logger, ITelegramBotClient bot)
    {
        _mediator = mediator;
        _telegram = telegram;
        _logger = logger;
        _bot = bot;
    }

    public bool CanHandle(Update update)
    {
        return update.Message?.Text == StartConstants.Command;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        var message = update.Message!;
        var tgUserId = message.From!.Id;

        _logger.LogInformation("Вызван /start ");

        var request = new StartRequest(tgUserId);

        var answer = await _mediator.Send(request, cancellationToken);
        var keyboard = new ReplyKeyboardMarkup(
            new[]
            {
                new KeyboardButton("Отправить номер телефона")
                {
                    RequestContact = true
                }
            })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };

        await _bot.SendMessage(
            chatId: message.Chat.Id,
            text: "Для регистрации отправьте свой номер телефона:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);

        return;
    }
}