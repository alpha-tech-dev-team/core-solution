using Atlas.Application.Features.Notifications;
using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Services;
using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreSolution.Integration.Tests.AtlasTests;

[Collection(nameof(IntegrationCollection))]
public sealed class IncidentNotificationTests(IntegrationFixture fixture)
{
    private const long AuthorChatId = 8401;
    private const long EngineerChatId = 8402;
    private const long TicketNumber = 1042;

    [Fact]
    public async Task NotifyIncidentAccepted_AuthorAndEngineer_NotifiesBothInOrder()
    {
        var notifications = new RecordingIncidentNotifier();

        await SendAsync(Accepted(CreateIncident(AuthorChatId, EngineerChatId)), notifications);

        Assert.Equal(new[] { AuthorChatId, EngineerChatId }, notifications.Alerts.Select(alert => alert.ChatId));
        Assert.Contains(TicketNumber.ToString(), notifications.Alerts[0].Text);
        Assert.Contains("принята", notifications.Alerts[0].Text);
    }

    [Fact]
    public async Task NotifyIncidentResolved_IncludesTicketNumberAndResolution()
    {
        const string resolution = "Заменена клавиатура.";
        var notifications = new RecordingIncidentNotifier();

        await SendAsync(Resolved(CreateIncident(AuthorChatId, null, resolution)), notifications);

        var alert = Assert.Single(notifications.Alerts);
        Assert.Contains(TicketNumber.ToString(), alert.Text);
        Assert.Contains(resolution, alert.Text);
    }

    [Fact]
    public async Task NotifyIncidentAccepted_RecipientWithoutTelegramUserId_IsSkipped()
    {
        var notifications = new RecordingIncidentNotifier();

        await SendAsync(Accepted(CreateIncident(null, EngineerChatId)), notifications);

        var alert = Assert.Single(notifications.Alerts);
        Assert.Equal(EngineerChatId, alert.ChatId);
    }

    [Fact]
    public async Task NotifyIncidentAccepted_AuthorIsEngineer_SendsSingleRecipient()
    {
        var notifications = new RecordingIncidentNotifier();

        await SendAsync(Accepted(CreateIncident(AuthorChatId, AuthorChatId)), notifications);

        var alert = Assert.Single(notifications.Alerts);
        Assert.Equal(AuthorChatId, alert.ChatId);
    }

    [Fact]
    public async Task NotifyIncidentAccepted_NoTelegramUserIds_SendsNothing()
    {
        var notifications = new RecordingIncidentNotifier();

        await SendAsync(Accepted(CreateIncident(null, null)), notifications);

        Assert.Empty(notifications.Alerts);
    }

    [Fact]
    public async Task NotifyIncidentAccepted_CancellationRequested_PropagatesCancellation()
    {
        var notifications = new RecordingIncidentNotifier();
        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => SendAsync(
            Accepted(CreateIncident(AuthorChatId, EngineerChatId)),
            notifications,
            cancellationSource.Token));

        Assert.Empty(notifications.Alerts);
    }

    [Fact]
    public async Task NotifyIncidentCreated_NotifiesAuthorWithCreatedText()
    {
        var notifications = new RecordingIncidentNotifier();

        var result = await SendAsync(
            new NotifyIncidentMessage(CreateIncident(AuthorChatId, null), IncidentNotificationType.Created),
            notifications);

        Assert.True(result.IsSuccess);

        var alert = Assert.Single(notifications.Alerts);
        Assert.Equal(AuthorChatId, alert.ChatId);
        Assert.Contains(TicketNumber.ToString(), alert.Text);
        Assert.Contains("создана", alert.Text);
    }

    [Fact]
    public async Task NotifyIncidentAccepted_OneRecipientFails_OtherStillNotified()
    {
        var notifications = new RecordingIncidentNotifier(failingChatId: AuthorChatId);

        var result = await SendAsync(Accepted(CreateIncident(AuthorChatId, EngineerChatId)), notifications);

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(ErrorCode.NotificationDeliveryFailed), result.ErrorCode);

        var alert = Assert.Single(notifications.Alerts);
        Assert.Equal(EngineerChatId, alert.ChatId);
    }

    private async Task<Result> SendAsync(
        NotifyIncidentMessage message,
        RecordingIncidentNotifier notifications,
        CancellationToken cancellationToken = default)
    {
        await using var atlas = fixture.CreateAtlasServiceProvider(
            configureServices: services => services.AddSingleton<IIncidentNotifier>(notifications));
        using var scope = atlas.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<IMediator>().Send(message, cancellationToken);
    }

    private static NotifyIncidentMessage Accepted(Incident incident)
    {
        return new NotifyIncidentMessage(incident, IncidentNotificationType.Accepted);
    }

    private static NotifyIncidentMessage Resolved(Incident incident)
    {
        return new NotifyIncidentMessage(incident, IncidentNotificationType.Resolved);
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
            AssignedEngineer = engineerTelegramUserId is null
                ? null
                : new Employee { TelegramUserId = engineerTelegramUserId },
            Resolution = resolution
        };
    }
}
