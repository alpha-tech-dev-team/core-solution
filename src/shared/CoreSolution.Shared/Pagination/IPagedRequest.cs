namespace CoreSolution.Shared.Pagination;

public interface IPagedRequest
{
    int? Page { get; }
    int? PageSize { get; }
}
