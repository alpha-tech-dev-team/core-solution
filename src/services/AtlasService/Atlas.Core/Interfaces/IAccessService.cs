namespace Atlas.Core.Interfaces;

public interface IAccessService
{
    Task<bool> IsAllowedAsync(long tgUserId, CancellationToken ct);
}