using CoreSolution.Shared.Application.Sorting;
using CoreSolution.Shared.Models;
using Pandora.Domain.Entities;

namespace Pandora.Application.Features.Secrets.Common;

public sealed class SecretSorting : SortFields<SecretMetadata>
{
    public static readonly SecretSorting Instance = new();

    private SecretSorting()
        : base(
            nameof(SecretMetadata.ResourceName),
            (nameof(SecretMetadata.ResourceName), metadata => metadata.ResourceName),
            (nameof(SecretMetadata.Username), metadata => metadata.Username))
    {
    }

    public static Func<IQueryable<SecretMetadata>, IOrderedQueryable<SecretMetadata>> For(
        string? sortBy,
        SortDirection? sortDirection) =>
        query => Instance.Apply(query, sortBy, sortDirection);

    public static IOrderedQueryable<SecretMetadata> ByResourceName(IQueryable<SecretMetadata> query) =>
        Instance.Apply(query, null, null);

    protected override IOrderedQueryable<SecretMetadata> TieBreak(IOrderedQueryable<SecretMetadata> ordered, string field) =>
        ordered.ThenBy(metadata => metadata.VaultItemId);
}
