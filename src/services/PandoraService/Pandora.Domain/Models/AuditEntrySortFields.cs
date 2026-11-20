using System.Linq.Expressions;
using Pandora.Domain.Entities;

namespace Pandora.Domain.Models;

public static class AuditEntrySortFields
{
    public const string Timestamp = nameof(AuditEntry.Timestamp);
    public const string Action = nameof(AuditEntry.Action);
    public const string RequestSource = nameof(AuditEntry.RequestSource);
    public const string TargetItemId = nameof(AuditEntry.TargetItemId);
    public const string Default = Timestamp;

    public static readonly IReadOnlyDictionary<string, Expression<Func<AuditEntry, object>>> Fields =
        new Dictionary<string, Expression<Func<AuditEntry, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            [Timestamp] = entry => entry.Timestamp,
            [Action] = entry => entry.Action,
            [RequestSource] = entry => entry.RequestSource,
            [TargetItemId] = entry => entry.TargetItemId!
        };

    public static string Resolve(string? sortBy)
    {
        if (sortBy is null) return Default;

        return Fields.Keys.FirstOrDefault(key => string.Equals(key, sortBy, StringComparison.OrdinalIgnoreCase))
            ?? Default;
    }
}
