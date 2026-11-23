namespace Atlas.Application.Abstractions;

public interface ITelegramMessageSender
{
    Task SendAsync(IReadOnlyCollection<long> chatIds, string text, CancellationToken cancellationToken = default);
}
