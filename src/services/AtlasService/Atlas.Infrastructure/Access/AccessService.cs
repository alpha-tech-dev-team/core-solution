using Atlas.Core.Interfaces;
using Atlas.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Access;

public sealed class AccessService : IAccessService
{
    private readonly HashSet<long>  _allowed;

    public AccessService(IOptions<AccessOptions> options)
    {
        _allowed = options.Value.AllowedChatIds.ToHashSet();
    }

    public Task<bool> IsAllowedAsync(long chatId, CancellationToken ctn)
    {
        return Task.FromResult(_allowed.Contains(chatId));
    }
}