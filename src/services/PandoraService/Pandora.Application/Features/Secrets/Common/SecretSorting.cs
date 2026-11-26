using System.Linq.Expressions;
using CoreSolution.Shared.Models;
using Pandora.Domain.Entities;

namespace Pandora.Application.Features.Secrets.Common;

public static class SecretSorting
{
    public const string DefaultField = nameof(SecretMetadata.ResourceName);

    private static readonly IReadOnlyDictionary<string, Expression<Func<SecretMetadata, string>>> SortFields =
        new Dictionary<string, Expression<Func<SecretMetadata, string>>>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(SecretMetadata.ResourceName)] = metadata => metadata.ResourceName,
            [nameof(SecretMetadata.Username)] = metadata => metadata.Username
        };

    public static readonly IReadOnlyCollection<string> SupportedFields = SortFields.Keys.ToArray();

    public static Func<IQueryable<SecretMetadata>, IOrderedQueryable<SecretMetadata>> For(
        string? sortBy,
        SortDirection? sortDirection)
    {
        var selector = SortFields[Resolve(sortBy)];
        var descending = sortDirection == SortDirection.Descending;

        return query => (descending
                ? query.OrderByDescending(selector)
                : query.OrderBy(selector))
            .ThenBy(metadata => metadata.VaultItemId);
    }

    public static IOrderedQueryable<SecretMetadata> ByResourceName(IQueryable<SecretMetadata> query)
    {
        return query.OrderBy(metadata => metadata.ResourceName).ThenBy(metadata => metadata.VaultItemId);
    }

    private static string Resolve(string? sortBy)
    {
        if (sortBy is null)
        {
            return DefaultField;
        }

        return SortFields.Keys.FirstOrDefault(key => string.Equals(key, sortBy, StringComparison.OrdinalIgnoreCase))
            ?? DefaultField;
    }
}
