namespace CoreSolution.Shared.Pagination;

public record PagedMessage
{
    public int? Page { get; init; }
    public int? PageSize { get; init; }

    public int EffectivePage => Page is null || Page < PaginationLimits.FirstPage
        ? PaginationLimits.FirstPage
        : Page.Value;

    public int EffectivePageSize => PageSize is null
        ? PaginationLimits.DefaultPageSize
        : Math.Clamp(PageSize.Value, PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize);

    public int Skip
    {
        get
        {
            var skip = ((long)EffectivePage - PaginationLimits.FirstPage) * EffectivePageSize;

            return skip > int.MaxValue ? int.MaxValue : (int)skip;
        }
    }
}
