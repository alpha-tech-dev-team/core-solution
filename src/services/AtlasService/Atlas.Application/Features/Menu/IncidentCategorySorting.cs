using Atlas.Domain.Entities;

namespace Atlas.Application.Features.Menu;

public static class IncidentCategorySorting
{
    public const string DefaultField = nameof(IncidentCategory.Name);

    public static IOrderedQueryable<IncidentCategory> Apply(IQueryable<IncidentCategory> query)
    {
        return query
            .OrderBy(category => category.Name)
            .ThenBy(category => category.Id);
    }
}
