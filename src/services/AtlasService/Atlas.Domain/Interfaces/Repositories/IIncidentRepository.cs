using Atlas.Domain.Entities;
using Atlas.Domain.Models;

namespace Atlas.Domain.Interfaces.Repositories;

public interface IIncidentRepository : IRepository<Incident>
{
    Task AddWithStatusHistoryAsync(Incident incident, CancellationToken cancellationToken = default);
    Task<IncidentListPage> GetListPage(IncidentPageQuery query, CancellationToken cancellationToken = default);
}
