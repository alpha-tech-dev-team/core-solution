namespace CoreSolution.Shared.Pagination;

public abstract record PagedMessage
{
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}
