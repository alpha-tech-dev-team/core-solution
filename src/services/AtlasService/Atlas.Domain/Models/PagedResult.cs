using Atlas.Domain.Constants;

namespace Atlas.Domain.Models;

public sealed record PagedResult<TItem>(IReadOnlyList<TItem> Items, int Page, int PageSize, int Total)
{
    public bool HasNextPage => (long)Page * PageSize < Total;
    public int NextPage => Page + PaginationLimits.PageStep;
}
