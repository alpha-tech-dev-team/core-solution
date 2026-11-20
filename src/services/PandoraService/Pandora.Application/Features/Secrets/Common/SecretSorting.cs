using Pandora.Domain.Entities;

namespace Pandora.Application.Features.Secrets.Common;

public static class SecretSorting
{
    public static IOrderedQueryable<SecretMetadata> ByResourceName(IQueryable<SecretMetadata> query)
    {
        return query.OrderBy(metadata => metadata.ResourceName).ThenBy(metadata => metadata.VaultItemId);
    }
}
