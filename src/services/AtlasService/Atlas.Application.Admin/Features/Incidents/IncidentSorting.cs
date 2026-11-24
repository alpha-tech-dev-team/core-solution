using Atlas.Domain.Entities;

namespace Atlas.Application.Features.Incidents;

public static class IncidentSorting
{
    public static IOrderedQueryable<Incident> ByNewest(IQueryable<Incident> query)
    {
        return query
            .OrderByDescending(incident => incident.CreatedAt)
            .ThenByDescending(incident => incident.TicketNumber)
            .ThenBy(incident => incident.Id);
    }
}
