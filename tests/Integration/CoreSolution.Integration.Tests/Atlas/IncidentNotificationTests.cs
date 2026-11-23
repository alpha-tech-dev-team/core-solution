using System.Net;
using System.Text.Json;
using Atlas.Application.Abstractions;
using Atlas.Application.Features.Notifications;
using Atlas.Domain.Entities;
using Atlas.TgBot.Notifications;
using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Telegram.Bot;
using Xunit;

namespace CoreSolution.Integration.Tests.AtlasTests;

[Collection(nameof(IntegrationCollection))]
public sealed class IncidentNotificationTests(IntegrationFixture fixture)
{
    private const long AuthorChatId = 8401;
    private const long EngineerChatId = 8402;
    private const long TicketNumber = 1042;

    [Fact]
    public void DependencyInjection_AtlasScope_ResolvesTelegramMessageSender()
    {
        using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();

        var sender = scope.ServiceProvider.GetRequiredService<ITelegramMessageSender>();

        Assert.IsType<TelegramMessageSender>(sender);
    }

    [Fact]
    public async Task NotifyIncidentAccepted_AuthorAndEngineer_NotifiesBothInOrder()
    {
        var notifications = new RecordingMessageSender();

        await SendAsync(new NotifyIncidentAcceptedMessage(CreateIncident(AuthorChatId, EngineerChatId)), notifications);

        var message = Assert.Single(notifications.Messages);
        Assert.Equal(new[] { AuthorChatId, EngineerChatId }, message.ChatIds);
        Assert.Contains(TicketNumber.ToString(), message.Text);
    }

    [Fact]
    public async Task NotifyIncidentAccepted_RecipientWithoutTelegramUserId_IsSkipped()
    {
        var notifications = new RecordingMessageSender();

        await SendAsync(new NotifyIncidentAcceptedMessage(CreateIncident(null, EngineerChatId)), notifications);

        var message = Assert.Single(notifications.Messages);
        Assert.Equal(new[] { EngineerChatId }, message.ChatIds);
    }

    [Fact]
    public async Task NotifyIncidentAccepted_NoTelegramUserIds_SendsNothing()
    {
        var notifications = new RecordingMessageSender();

        await SendAsync(new NotifyIncidentAcceptedMessage(CreateIncident(null, null)), notifications);

        Assert.Empty(notifications.Messages);
    }

    [Fact]
    public async Task NotifyIncidentAccepted_AuthorIsEngineer_SendsSingleRecipient()
    {
        var notifications = new RecordingMessageSender();

        await SendAsync(new NotifyIncidentAcceptedMessage(CreateIncident(AuthorChatId, AuthorChatId)), notifications);

        var message = Assert.Single(notifications.Messages);
        Assert.Equal(new[] { AuthorChatId }, message.ChatIds);
    }

    [Fact]
    public async Task NotifyIncidentResolved_IncludesTicketNumberAndResolution()
    {
        const string resolution = "Заменена клавиатура.";
        var notifications = new RecordingMessageSender();

        await SendAsync(
            new NotifyIncidentResolvedMessage(CreateIncident(AuthorChatId, null, resolution)),
            notifications);

        var message = Assert.Single(notifications.Messages);
        Assert.Contains(TicketNumber.ToString(), message.Text);
        Assert.Contains(resolution, message.Text);
    }

    [Fact]
    public async Task NotifyIncidentResolved_NoResolution_SendsEmptyResolution()
    {
        var notifications = new RecordingMessageSender();

        await SendAsync(new NotifyIncidentResolvedMessage(CreateIncident(AuthorChatId, null)), notifications);

        var message = Assert.Single(notifications.Messages);
        Assert.Contains(TicketNumber.ToString(), message.Text);
    }

    [Fact]
    public async Task NotifyIncidentAccepted_CancellationRequested_PropagatesCancellation()
    {
        var notifications = new RecordingMessageSender();
        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => SendAsync(
            new NotifyIncidentAcceptedMessage(CreateIncident(AuthorChatId, EngineerChatId)),
            notifications,
            cancellationSource.Token));

        Assert.Empty(notifications.Messages);
    }

    [Fact]
    public async Task SendAsync_OneRecipientFails_OtherStillAttempted()
    {
        var telegram = new StubTelegramHandler(AuthorChatId);
        var sender = new TelegramMessageSender(
            new TelegramBotClient(IntegrationFixture.AtlasNotificationTestToken, new HttpClient(telegram)),
            NullLogger<TelegramMessageSender>.Instance);

        await sender.SendAsync(new[] { AuthorChatId, EngineerChatId }, "text");

        Assert.Equal(new[] { AuthorChatId, EngineerChatId }, telegram.ChatIds);
    }

    private async Task SendAsync(
        IRequest<Result> message,
        RecordingMessageSender notifications,
        CancellationToken cancellationToken = default)
    {
        await using var atlas = fixture.CreateAtlasServiceProvider(services =>
            services.Replace(ServiceDescriptor.Singleton<ITelegramMessageSender>(notifications)));
        using var scope = atlas.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IMediator>().Send(message, cancellationToken);
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

    private sealed class RecordingMessageSender : ITelegramMessageSender
    {
        public List<(IReadOnlyCollection<long> ChatIds, string Text)> Messages { get; } = [];

        public Task SendAsync(
            IReadOnlyCollection<long> chatIds,
            string text,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Messages.Add((chatIds, text));
            return Task.CompletedTask;
        }
    }

    private sealed class StubTelegramHandler(long failingChatId) : HttpMessageHandler
    {
        public List<long> ChatIds { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            using var json = JsonDocument.Parse(body);
            var chatId = json.RootElement.GetProperty("chat_id").GetInt64();
            ChatIds.Add(chatId);

            var span = chatId == failingChatId
                ? """{"ok":false,"error_code":400,"description":"Bad Request: chat not found"}"""
                : """{"ok":true,"result":{"message_id":1,"date":1700000000,"chat":{"id":1,"type":"private"}}}""";

            return new HttpResponseMessage(chatId == failingChatId ? HttpStatusCode.BadRequest : HttpStatusCode.OK)
            {
                Content = new StringContent(span)
            };
        }
    }
}
