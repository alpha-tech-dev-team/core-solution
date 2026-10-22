using Microsoft.Extensions.Options;
using Pandora.Core.Interfaces;

namespace Pandora.Infrastructure.Access;

public sealed class IdentityAccessService : IAccessService
{
    private readonly IIdentityClient _identityClient;
    private readonly IOptionsMonitor<ServiceCatalogOptions> _options;

    public IdentityAccessService(IIdentityClient identityClient, IOptionsMonitor<ServiceCatalogOptions> options)
    {
        _identityClient = identityClient;
        _options = options;
    }

    public async Task<IReadOnlyList<string>> GetServices(long chatId, CancellationToken cancellationToken)
    {
        var catalog = _options.CurrentValue.Items;
        var services = new List<string>(catalog.Count);

        foreach (var service in catalog)
        {
            if (await _identityClient.CheckUser(chatId, service, cancellationToken))
            {
                services.Add(service);
            }
        }

        return services;
    }
}
