using Microsoft.Extensions.Options;
using Pandora.Core.Interfaces;

namespace Pandora.Infrastructure.Access;

public sealed class ConfigAccessService : IAccessService
{
    private readonly IOptionsMonitor<AccessOptions> _options;

    public ConfigAccessService(IOptionsMonitor<AccessOptions> options)
    {
        _options = options;
    }

    public Task<bool> HasAccessAsync(long chatId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_options.CurrentValue.AllowedChatIds.Contains(chatId));
    }
}
