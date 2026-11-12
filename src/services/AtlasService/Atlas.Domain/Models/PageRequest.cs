using Atlas.Domain.Constants;

namespace Atlas.Domain.Models;

public sealed record PageRequest
{
    public PageRequest(int page, int pageSize)
    {
        Page = page < PaginationLimits.FirstPage ? PaginationLimits.FirstPage : page;
        PageSize = pageSize < PaginationLimits.MinPageSize
            ? PaginationLimits.DefaultPageSize
            : Math.Min(pageSize, PaginationLimits.MaxPageSize);
    }

    public int Page { get; }

    public int PageSize { get; }

    public int Skip => (Page - PaginationLimits.FirstPage) * PageSize;
}
