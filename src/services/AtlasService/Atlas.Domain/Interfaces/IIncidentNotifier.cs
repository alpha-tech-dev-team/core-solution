namespace Atlas.Domain.Interfaces;

public interface IIncidentNotifier
{
    Task SendAsync(long chatId, string text, CancellationToken cancellationToken = default);
}
