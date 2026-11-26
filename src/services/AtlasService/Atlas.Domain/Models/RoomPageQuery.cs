using Atlas.Domain.Entities;
using CoreSolution.Shared.Models;

namespace Atlas.Domain.Models;

public sealed record RoomPageQuery(
    PagedMessage Paging,
    Func<IQueryable<Room>, IOrderedQueryable<Room>> OrderBy);
