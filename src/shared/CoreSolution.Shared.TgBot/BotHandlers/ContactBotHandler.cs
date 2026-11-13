using CoreSolution.Shared.Application.Messages;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.Extensions;
using MediatR;
using Telegram.Bot.Types;

namespace CoreSolution.Shared.TgBot.BotHandlers;

public sealed class ContactBotHandler : IBotHandler
{
    private const string StartCommand = "/start";
    private const int MinPhoneDigits = 7;

    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly IMediator _mediator;
    private readonly IUserErrorMapper _userErrorMapper;

    public ContactBotHandler(
        ITelegramNotificationService telegramNotificationService,
        IMediator mediator,
        IUserErrorMapper userErrorMapper)
    {
        _telegramNotificationService = telegramNotificationService;
        _mediator = mediator;
        _userErrorMapper = userErrorMapper;
    }

    public bool CanHandle(Update update)
    {
        var message = update.Message;

        return message?.Contact is not null || message?.Text is StartCommand || IsPhoneText(message?.Text);
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { Chat.Id: long tgUserId, MessageId: int messageId } message)
        {
            return;
        }

        var phone = ExtractOwnedPhone(message);

        var result = await _mediator.Send(new SubmitPhoneMessage(tgUserId, messageId, phone), cancellationToken);

        await _telegramNotificationService.SendMessageAsync(
            tgUserId,
            result.GetUserText(_userErrorMapper),
            result.ToTelegramMarkup(),
            cancellationToken);
    }

    private static string? ExtractOwnedPhone(Message message)
    {
        if (message.Contact is { } contact)
        {
            return contact.UserId is not null && contact.UserId == message.From?.Id
                ? contact.PhoneNumber
                : null;
        }

        return IsPhoneText(message.Text) ? message.Text : null;
    }

    private static bool IsPhoneText(string? text)
    {
        return text is not null && text.Count(char.IsDigit) >= MinPhoneDigits;
    }
}
