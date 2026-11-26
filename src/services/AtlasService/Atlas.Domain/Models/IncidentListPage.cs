namespace Atlas.Domain.Models;

public sealed record IncidentListPage(
    IReadOnlyList<IncidentPageItem> Items,
    int Page,
    int PageSize,
    int Total);
