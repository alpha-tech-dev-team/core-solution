using System.Data.Common;
using System.Runtime.CompilerServices;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;
using Atlas.Core.Commands;
using Telegram.Bot.Types.Enums;


namespace Atlas.App.Services;

public class UpdateHandler
{
    private readonly IMediator _mediator;

    public UpdateHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ctn)
    {
        if (update.Type == UpdateType.Message && update.Message?.Text is { } messageText)
        {
            long chatId = update.Message.Chat.Id;
            string text = update.Message.Text;
            string name = update.Message.From?.FirstName ?? "пользоватеь";

            var command = new TextCommand(chatId, text, name);

            string answerText = await _mediator.Send(command, ctn);

            await botClient.SendMessage(
                chatId: chatId,
                text: answerText,
                cancellationToken: ctn
            );
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ctn)
    {
        Console.WriteLine($"Ошибка Telegram API: {exception.Message}");
        return Task.CompletedTask;
    }
}