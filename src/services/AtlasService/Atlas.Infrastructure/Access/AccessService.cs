using Atlas.Core.Enums;
using Atlas.Core.Interfaces;
using Atlas.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Access;

public sealed class AccessService : IAccessService
{
    private readonly HashSet<long> _allowed;

    public AccessService(IOptions<AccessOptions> options)
    {
        _allowed = options.Value.AllowedTgUserIds.ToHashSet();
    }
    public Task<bool> IsAllowedAsync(long tgUserId, CancellationToken cancellationToken)
    {
        var allowed = _allowed.Contains(tgUserId);
        return Task.FromResult(allowed);
    }
}