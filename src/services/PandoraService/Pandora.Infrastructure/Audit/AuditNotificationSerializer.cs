using System.Text.Json;
using Pandora.Domain.Models;

namespace Pandora.Infrastructure.Audit;

public static class AuditNotificationSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static string Serialize(AuditEntryNotification notification)
    {
        return JsonSerializer.Serialize(notification, Options);
    }

    public static AuditEntryNotification? Deserialize(string payload)
    {
        return JsonSerializer.Deserialize<AuditEntryNotification>(payload, Options);
    }
}
