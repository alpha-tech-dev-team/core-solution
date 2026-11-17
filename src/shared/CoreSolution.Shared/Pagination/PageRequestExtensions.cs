namespace CoreSolution.Shared.Pagination;

public static class PageRequestExtensions
{
    public static PageRequest ToPageRequest(this PagedMessage message)
    {
        return new PageRequest(
            message.Page ?? PaginationLimits.DefaultPage,
            message.PageSize ?? PaginationLimits.DefaultPageSize);
    }
}
