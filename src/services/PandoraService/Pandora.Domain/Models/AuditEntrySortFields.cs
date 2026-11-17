using System.Linq.Expressions;
using Pandora.Domain.Entities;

namespace Pandora.Domain.Models;

public static class AuditEntrySortFields
{
    public const string Timestamp = "timestamp";
    public const string Action = "action";
    public const string RequestSource = "requestSource";
    public const string TargetItemId = "targetItemId";

    public const string Default = Timestamp;

    public static readonly IReadOnlyDictionary<string, Expression<Func<AuditEntry, object>>> Fields =
        new Dictionary<string, Expression<Func<AuditEntry, object>>>
        {
            [Timestamp] = entry => entry.Timestamp,
            [Action] = entry => entry.Action,
            [RequestSource] = entry => entry.RequestSource,
            [TargetItemId] = entry => entry.TargetItemId!
        };
}
