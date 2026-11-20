namespace CoreSolution.Shared.Models;

public interface ISortMessage
{
    string? SortBy { get; }
    SortDirection? SortDirection { get; }
}

public abstract record PagedSortMessage(
    int? Page = null,
    int? PageSize = null,
    string? SortBy = null,
    SortDirection? SortDirection = null)
    : PagedMessage(Page, PageSize), ISortMessage;
