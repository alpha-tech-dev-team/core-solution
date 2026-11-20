using Atlas.Domain.Entities;
using Atlas.TgBot.Notifications;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.TgBot.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;

namespace CoreSolution.Integration.Tests.AtlasTests;

[Collection(nameof(IntegrationCollection))]
public sealed class IncidentNotificationTests(IntegrationFixture fixture)
{
    private const long AuthorChatId = 8401;
    private const long EngineerChatId = 8402;
    private const long TicketNumber = 1042;

    [Fact]
    public async Task NotifyAcceptedAsync_AuthorAndEngineer_NotifiesBothInOrder()
    {
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();
        var notifications = new RecordingNotificationService();
        var service = CreateService(scope, notifications);

        await service.NotifyAcceptedAsync(CreateIncident(AuthorChatId, EngineerChatId));

        Assert.Equal(
            new[] { AuthorChatId, EngineerChatId },
            notifications.Messages.Select(message => message.ChatId));
        Assert.All(
            notifications.Messages,
            message => Assert.Contains(TicketNumber.ToString(), message.Text));
    }

    [Fact]
    public async Task NotifyAcceptedAsync_RecipientWithoutTelegramUserId_IsSkipped()
    {
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();
        var notifications = new RecordingNotificationService();
        var service = CreateService(scope, notifications);

        await service.NotifyAcceptedAsync(CreateIncident(null, EngineerChatId));

        var message = Assert.Single(notifications.Messages);
        Assert.Equal(EngineerChatId, message.ChatId);
    }

    [Fact]
    public async Task NotifyAcceptedAsync_NoTelegramUserIds_SendsNothing()
    {
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();
        var notifications = new RecordingNotificationService();
        var service = CreateService(scope, notifications);

        await service.NotifyAcceptedAsync(CreateIncident(null, null));

        Assert.Empty(notifications.Messages);
    }

    [Fact]
    public async Task NotifyAcceptedAsync_AuthorIsEngineer_SendsSingleMessage()
    {
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();
        var notifications = new RecordingNotificationService();
        var service = CreateService(scope, notifications);

        await service.NotifyAcceptedAsync(CreateIncident(AuthorChatId, AuthorChatId));

        var message = Assert.Single(notifications.Messages);
        Assert.Equal(AuthorChatId, message.ChatId);
    }

    [Fact]
    public async Task NotifyResolvedAsync_IncludesTicketNumberAndResolution()
    {
        const string resolution = "Заменена клавиатура.";
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();
        var notifications = new RecordingNotificationService();
        var service = CreateService(scope, notifications);

        await service.NotifyResolvedAsync(CreateIncident(AuthorChatId, null, resolution));

        var message = Assert.Single(notifications.Messages);
        Assert.Contains(TicketNumber.ToString(), message.Text);
        Assert.Contains(resolution, message.Text);
    }

    [Fact]
    public async Task NotifyAcceptedAsync_OneRecipientFails_OtherStillNotified()
    {
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();
        var notifications = new RecordingNotificationService(AuthorChatId);
        var service = CreateService(scope, notifications);

        await service.NotifyAcceptedAsync(CreateIncident(AuthorChatId, EngineerChatId));

        var message = Assert.Single(notifications.Messages);
        Assert.Equal(EngineerChatId, message.ChatId);
    }

    [Fact]
    public async Task NotifyAcceptedAsync_CancellationRequested_PropagatesCancellation()
    {
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();
        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();
        var notifications = new RecordingNotificationService();
        var service = CreateService(scope, notifications);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.NotifyAcceptedAsync(
                CreateIncident(AuthorChatId, EngineerChatId),
                cancellationSource.Token));

        Assert.Empty(notifications.Messages);
    }

    private static IncidentNotificationService CreateService(
        IServiceScope scope,
        RecordingNotificationService notifications)
    {
        return new IncidentNotificationService(
            notifications,
            scope.ServiceProvider.GetRequiredService<ILocalizer>(),
            NullLogger<IncidentNotificationService>.Instance);
    }

    private static Incident CreateIncident(
        long? authorTelegramUserId,
        long? engineerTelegramUserId,
        string? resolution = null)
    {
        return new Incident
        {
            Id = Guid.NewGuid(),
            TicketNumber = TicketNumber,
            Employee = new Employee { TelegramUserId = authorTelegramUserId },
            AssignedEngineer = new Employee { TelegramUserId = engineerTelegramUserId },
            Resolution = resolution
        };
    }

    private sealed class RecordingNotificationService(long? failingChatId = null) : ITelegramNotificationService
    {
        public List<(long ChatId, string Text)> Messages { get; } = [];

        public Task<Message> SendMessageAsync(
            long chatId,
            string text,
            ReplyMarkup? replyMarkup = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (chatId == failingChatId)
            {
                throw new InvalidOperationException("Telegram send failed.");
            }

            Messages.Add((chatId, text));

            return Task.FromResult(new Message());
        }

        public Task<Message> SendAlertAsync(
            long chatId,
            string text,
            ReplyMarkup? replyMarkup = null,
            CancellationToken cancellationToken = default)
        {
            return SendMessageAsync(chatId, text, replyMarkup, cancellationToken);
        }

        public Task<Message> EditMessageAsync(
            long chatId,
            int messageId,
            string text,
            InlineKeyboardMarkup? replyMarkup = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Message());
        }

        public Task DeleteMessageAsync(
            long chatId,
            int messageId,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
