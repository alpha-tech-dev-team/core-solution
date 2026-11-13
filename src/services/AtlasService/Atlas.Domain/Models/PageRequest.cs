using Atlas.Domain.Constants;

namespace Atlas.Domain.Models;

public sealed record PageRequest
{
    public int Page { get; }
    public int PageSize { get; }
    public int Skip => (Page - PaginationLimits.FirstPage) * PageSize;

    public PageRequest(int page, int pageSize)
    {
        Page = page < PaginationLimits.FirstPage ? PaginationLimits.FirstPage : page;
        PageSize = Math.Clamp(pageSize, PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize);
    }
}
