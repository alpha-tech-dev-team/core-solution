using Atlas.Domain.Constants.AtlasDBConfiguration;
using Atlas.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Atlas.Infrastructure.Database.Converters;

public sealed class IncidentStatusConverter : ValueConverter<IncidentStatus, string>
{
    public IncidentStatusConverter()
        : base(status => ToProvider(status), value => FromProvider(value))
    {
    }

    private static string ToProvider(IncidentStatus status) => status switch
    {
        IncidentStatus.Created => DBKeys.Incident.StatusCreated,
        IncidentStatus.InProgress => DBKeys.Incident.StatusInProgress,
        IncidentStatus.Resolved => DBKeys.Incident.StatusResolved,
        IncidentStatus.ResolvedAutomatically => DBKeys.Incident.StatusResolvedAutomatically,
        IncidentStatus.Closed => DBKeys.Incident.StatusClosed,
        IncidentStatus.Cancelled => DBKeys.Incident.StatusCancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    private static IncidentStatus FromProvider(string value) => value switch
    {
        DBKeys.Incident.StatusCreated => IncidentStatus.Created,
        DBKeys.Incident.StatusInProgress => IncidentStatus.InProgress,
        DBKeys.Incident.StatusResolved => IncidentStatus.Resolved,
        DBKeys.Incident.StatusResolvedAutomatically => IncidentStatus.ResolvedAutomatically,
        DBKeys.Incident.StatusClosed => IncidentStatus.Closed,
        DBKeys.Incident.StatusCancelled => IncidentStatus.Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };
}
