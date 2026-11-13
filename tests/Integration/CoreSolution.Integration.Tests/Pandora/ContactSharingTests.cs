using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Application.Messages;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.BotHandlers;
using CoreSolution.Shared.TgBot.Extensions;
using Identity.Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Application.Features.Credentials;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

[Collection(nameof(IntegrationCollection))]
public sealed class ContactSharingTests(IntegrationFixture fixture)
{
    [Fact]
    public void ToTelegramMarkup_RequestContact_ReturnsContactKeyboard()
    {
        var result = Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = "Отправьте номер телефона",
            RequestContact = true,
            ContactButtonText = "Отправить номер телефона"
        });

        var markup = result.ToTelegramMarkup();

        var keyboard = Assert.IsType<ReplyKeyboardMarkup>(markup);
        var button = Assert.Single(keyboard.Keyboard.SelectMany(row => row));
        Assert.Equal("Отправить номер телефона", button.Text);
        Assert.True(button.RequestContact);
    }

    [Fact]
    public async Task SubmitPhone_WithoutPhone_ResponseCarriesContactButtonText()
    {
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SubmitPhoneMessage(8200, 1, null));

        Assert.True(result.Value!.RequestContact);
        Assert.Equal("Отправить номер телефона", result.Value.ContactButtonText);
    }

    [Fact]
    public async Task ContactBotHandler_CanHandle_AcceptsTypedPhoneAndStartOnly()
    {
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();

        var handler = scope.ServiceProvider.GetServices<IBotHandler>()
            .OfType<ContactBotHandler>()
            .Single();

        Assert.True(handler.CanHandle(CreateMessageUpdate("+375333057384")));
        Assert.True(handler.CanHandle(CreateMessageUpdate("/start")));
        Assert.False(handler.CanHandle(CreateMessageUpdate("привет")));
    }

    private static Update CreateMessageUpdate(string text)
    {
        return new Update
        {
            Message = new Message { Text = text, Chat = new Chat { Id = 123 } }
        };
    }

    private static Update CreateMessageUpdate(long senderId, string text)
    {
        return new Update
        {
            Message = new Message
            {
                Text = text,
                Chat = new Chat { Id = senderId },
                From = new User { Id = senderId }
            }
        };
    }

    [Fact]
    public async Task ContactBotHandler_OwnContact_ActivatesUser()
    {
        const long senderId = 8301;
        const string phone = "+79995550301";
        await fixture.SeedPendingUserAsync(phone);
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();
        var notifications = new RecordingNotificationService();
        var handler = CreateHandler(scope, notifications);

        await handler.HandleAsync(CreateContactUpdate(senderId, senderId, phone), CancellationToken.None);

        var state = await fixture.GetUserStateAsync(phone);
        Assert.Equal(senderId, state.TelegramUserId);
        Assert.Equal(UserStatus.Active, state.Status);
        Assert.False(notifications.LastMessageRequestedContact);
    }

    [Fact]
    public async Task ContactBotHandler_ForeignContact_DoesNotActivateAndRequestsOwnContact()
    {
        const long senderId = 8302;
        const string phone = "+79995550302";
        await fixture.SeedPendingUserAsync(phone);
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();
        var notifications = new RecordingNotificationService();
        var handler = CreateHandler(scope, notifications);

        await handler.HandleAsync(CreateContactUpdate(senderId, senderId + 1, phone), CancellationToken.None);

        var state = await fixture.GetUserStateAsync(phone);
        Assert.Null(state.TelegramUserId);
        Assert.Equal(UserStatus.Pending, state.Status);
        Assert.True(notifications.LastMessageRequestedContact);
    }

    [Fact]
    public async Task ContactBotHandler_BlockedUserResharingContact_StaysBlockedAndToldSo()
    {
        const long senderId = 8304;
        const string phone = "+79995550304";
        await fixture.SeedPendingUserAsync(phone);
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();
        var notifications = new RecordingNotificationService();
        var handler = CreateHandler(scope, notifications);
        await handler.HandleAsync(CreateContactUpdate(senderId, senderId, phone), CancellationToken.None);
        await fixture.SetUserStatusAsync(phone, UserStatus.Blocked);

        await handler.HandleAsync(CreateContactUpdate(senderId, senderId, phone), CancellationToken.None);

        var state = await fixture.GetUserStateAsync(phone);
        Assert.Equal(UserStatus.Blocked, state.Status);
        Assert.Equal("Ваш аккаунт заблокирован. Обратитесь к администратору.", notifications.LastMessageText);
    }
    [Fact]
    public async Task BlockedUserWithoutSession_GetsBlockedMessageInsteadOfPhoneRequest()
    {
        const long telegramUserId = 8305;
        const string phone = "+79995550305";

        await fixture.SeedPendingUserAsync(phone, "Pandora");
        await fixture.SetTelegramUserAsync(phone, telegramUserId, UserStatus.Blocked);
        await using var provider = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ISessionService>()
            .DeleteSession(telegramUserId, CancellationToken.None);

        var result = await scope.ServiceProvider.GetRequiredService<IMediator>()
            .Send(new GetCredentialsMessage(telegramUserId, MessageId: 1, ItemId: "TestService"), CancellationToken.None);

        Assert.Equal("UserBlocked", result.ErrorCode);
    }

    [Fact]
    public async Task ContactBotHandler_TypedPhone_DoesNotActivateAndRequestsOwnContact()
    {
        const long senderId = 8303;
        const string phone = "+79995550303";
        await fixture.SeedPendingUserAsync(phone);
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();
        var notifications = new RecordingNotificationService();
        var handler = CreateHandler(scope, notifications);

        await handler.HandleAsync(CreateMessageUpdate(senderId, phone), CancellationToken.None);

        var state = await fixture.GetUserStateAsync(phone);
        Assert.Null(state.TelegramUserId);
        Assert.Equal(UserStatus.Pending, state.Status);
        Assert.True(notifications.LastMessageRequestedContact);
    }

    private static ContactBotHandler CreateHandler(IServiceScope scope, RecordingNotificationService notifications)
    {
        return new ContactBotHandler(
            notifications,
            scope.ServiceProvider.GetRequiredService<IMediator>(),
            scope.ServiceProvider.GetRequiredService<IUserErrorMapper>());
    }

    private static Update CreateContactUpdate(long senderId, long contactOwnerId, string phone)
    {
        return new Update
        {
            Message = new Message
            {
                Chat = new Chat { Id = senderId },
                From = new User { Id = senderId },
                Contact = new Contact { PhoneNumber = phone, FirstName = "Test", UserId = contactOwnerId }
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

        public Task DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
