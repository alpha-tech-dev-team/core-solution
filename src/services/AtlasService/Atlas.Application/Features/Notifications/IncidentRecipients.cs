using Atlas.Domain.Entities;

namespace Atlas.Application.Features.Notifications;

internal static class IncidentRecipients
{
    public static List<long> From(Incident incident)
    {
        return new[]
            {
                incident.Employee?.TelegramUserId,
                incident.AssignedEngineer?.TelegramUserId
            }
            .Where(chatId => chatId.HasValue)
            .Select(chatId => chatId.GetValueOrDefault())
            .Distinct()
            .ToList();
    }
}
