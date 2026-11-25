using Atlas.Domain.Interfaces.Services;

namespace CoreSolution.Integration.Tests.AtlasTests;

internal sealed class RecordingIncidentNotifier(long? failingChatId = null) : IIncidentNotifier
{
    public List<(long ChatId, string Text)> Alerts { get; } = [];

    public List<long> AttemptedChatIds { get; } = [];

    public Task SendAsync(long chatId, string text, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        AttemptedChatIds.Add(chatId);

        if (chatId == failingChatId)
        {
            throw new InvalidOperationException($"Chat {chatId} is unavailable.");
        }

        Alerts.Add((chatId, text));
        return Task.CompletedTask;
    }
}
