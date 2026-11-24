using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.BotHandlers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;

namespace CoreSolution.Integration.Tests.AtlasTests;

[Collection(nameof(IntegrationCollection))]
public sealed class DevelopmentPhoneConfirmationSkipTests(IntegrationFixture fixture)
{
    private const string PhoneRequestText = "Отправьте номер телефона";

    [Fact]
    public async Task ContactBotHandler_WithoutPhoneConfirmation_SkipsPhoneRequestAndShowsMenu()
    {
        await using var atlas = fixture.CreateAtlasServiceProvider(new Dictionary<string, string?>
        {
            ["PhoneConfirmation:Enabled"] = "false"
        });

        using var scope = atlas.CreateScope();
        var notifications = new RecordingNotificationService();
        var handler = new ContactBotHandler(
            notifications,
            scope.ServiceProvider.GetRequiredService<IMediator>(),
            scope.ServiceProvider.GetRequiredService<IUserErrorMapper>());

        await handler.HandleAsync(CreateStartUpdate(8501), CancellationToken.None);

        Assert.NotNull(notifications.LastMessageText);
        Assert.DoesNotContain(PhoneRequestText, notifications.LastMessageText);
        Assert.False(notifications.LastMessageRequestedContact);
    }

    private static Update CreateStartUpdate(long telegramUserId)
    {
        return new Update
        {
            Id = 1,
            Message = new Message
            {
                Text = "/start",
                Chat = new Chat { Id = telegramUserId },
                From = new User { Id = telegramUserId }
            }
        };
    }

    private sealed class RecordingNotificationService : ITelegramNotificationService
    {
        public bool LastMessageRequestedContact { get; private set; }

        public string? LastMessageText { get; private set; }

        public Task<Message> SendMessageAsync(long chatId, string text, ReplyMarkup? replyMarkup = null, CancellationToken cancellationToken = default)
        {
            LastMessageText = text;
            LastMessageRequestedContact = replyMarkup is ReplyKeyboardMarkup keyboard
                && keyboard.Keyboard.SelectMany(row => row).Any(button => button.RequestContact);

            return Task.FromResult(new Message());
        }

        public Task<Message> SendAlertAsync(long chatId, string text, ReplyMarkup? replyMarkup = null, CancellationToken cancellationToken = default)
        {
            return SendMessageAsync(chatId, text, replyMarkup, cancellationToken);
        }

        public Task<Message> EditMessageAsync(long chatId, int messageId, string text, InlineKeyboardMarkup? replyMarkup = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Message());
        }

        public Task DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
