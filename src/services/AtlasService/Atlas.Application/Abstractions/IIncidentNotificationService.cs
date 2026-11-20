using Atlas.Domain.Entities;

namespace Atlas.Application.Abstractions;

public interface IIncidentNotificationService
{
    Task NotifyAcceptedAsync(Incident incident, CancellationToken cancellationToken = default);
    Task NotifyResolvedAsync(Incident incident, CancellationToken cancellationToken = default);
}
