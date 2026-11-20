using Atlas.Domain.Entities;

namespace Atlas.Domain.Interfaces.Repositories;

public interface IIncidentRepository : IRepository<Incident>
{
    Task AddWithStatusHistoryAsync(Incident incident, CancellationToken cancellationToken = default);
}
