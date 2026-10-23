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
        _allowed = options.Value.AllowedChatIds.ToHashSet();
        _logger = logger;
        _logger.LogInformation("AccessService initialized. Allowed chat IDs: {Allowed}", string.Join(", ", _allowed));
    }

    public Task<bool> IsAllowedAsync(long chatId, CancellationToken ct)
    {
        var allowed = _allowed.Contains(chatId);
        _logger.LogInformation("Access check for chat_id {ChatId}: {Result}", chatId, allowed ? "ALLOWED" : "DENIED");
        return Task.FromResult(allowed);
    }
}