using Atlas.Domain.Models;

namespace Atlas.Domain.Interfaces;

public interface IDraftStore
{
    Task<IncidentDraft?> GetAsync(long telegramUserId, CancellationToken cancellationToken = default);
    Task SaveAsync(long telegramUserId, IncidentDraft draft, CancellationToken cancellationToken = default);
    Task ClearAsync(long telegramUserId, CancellationToken cancellationToken = default);
}
