namespace Atlas.Application.Features.Notifications;

public interface IIncidentNotifier
{
    Task SendAsync(long chatId, string text, CancellationToken cancellationToken = default);
}
