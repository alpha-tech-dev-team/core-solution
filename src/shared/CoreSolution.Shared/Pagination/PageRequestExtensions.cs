namespace CoreSolution.Shared.Pagination;

public static class PageRequestExtensions
{
    public static PageRequest ToPageRequest(this IPagedRequest request)
    {
        return new PageRequest(
            request.Page ?? PaginationLimits.DefaultPage,
            request.PageSize ?? PaginationLimits.DefaultPageSize);
    }
}
