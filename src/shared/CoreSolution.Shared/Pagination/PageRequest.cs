namespace CoreSolution.Shared.Pagination;

public sealed record PageRequest
{
    public int Page { get; }
    public int PageSize { get; }
    public int Skip { get; }

    public PageRequest(int page, int pageSize)
    {
        Page = page < PaginationLimits.FirstPage ? PaginationLimits.FirstPage : page;
        PageSize = Math.Clamp(pageSize, PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize);

        var skip = ((long)Page - PaginationLimits.FirstPage) * PageSize;
        Skip = skip > int.MaxValue ? int.MaxValue : (int)skip;
    }
}
