using Atlas.Core.Enums;
using Atlas.Core.Interfaces;
using Atlas.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Access;

public sealed class AccessService : IAccessService
{
    private readonly HashSet<long> _allowed;
    private readonly ILogger<AccessService> _logger;

    public AccessService(IOptions<AccessOptions> options, ILogger<AccessService> logger)
    {
        _allowed = options.Value.AllowedTgUserIds.ToHashSet();
        _logger = logger;
        _logger.LogInformation("AccessService initialized. Allowed TgUserIds: {Count}", _allowed.Count);
    }

    public Task<bool> IsAllowedAsync(long tgUserId, CancellationToken cancellationToken)
    {
        var allowed = _allowed.Contains(tgUserId);
        _logger.LogInformation("Access check for TgUserId {TgUserId}: {Result}", tgUserId, allowed ? "ALLOWED" : "DENIED");
        return Task.FromResult(allowed);
    }
}