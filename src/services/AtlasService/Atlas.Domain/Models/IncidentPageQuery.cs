using Atlas.Domain.Entities;
using CoreSolution.Shared.Models;

namespace Atlas.Domain.Models;

public sealed record IncidentPageQuery(
    PagedMessage Paging,
    Func<IQueryable<Incident>, IOrderedQueryable<Incident>> OrderBy);
