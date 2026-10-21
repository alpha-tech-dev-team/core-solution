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

    public Task<IReadOnlyList<string>> GetServices(long chatId, CancellationToken cancellationToken)
    {
        var user = _options.CurrentValue.Users.FirstOrDefault(candidate => candidate.ChatId == chatId);
        IReadOnlyList<string> services = user?.Services ?? [];

        return Task.FromResult(services);
    }
}
