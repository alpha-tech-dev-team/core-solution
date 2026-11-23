namespace Atlas.Domain.Interfaces.Services;

public interface IIncidentNotifier
{
    Task SendAsync(long chatId, string text, CancellationToken cancellationToken = default);
}
