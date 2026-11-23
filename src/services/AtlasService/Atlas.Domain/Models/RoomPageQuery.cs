using Atlas.Domain.Entities;
using CoreSolution.Shared.Pagination;

namespace Atlas.Domain.Models;

public sealed record RoomPageQuery(
    PagedMessage Paging,
    Func<IQueryable<Room>, IOrderedQueryable<Room>> OrderBy);
