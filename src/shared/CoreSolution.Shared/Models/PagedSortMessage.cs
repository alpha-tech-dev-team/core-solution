namespace CoreSolution.Shared.Models;

public abstract record PagedSortMessage(
    int? Page = null,
    int? PageSize = null,
    string? SortBy = null,
    SortDirection? SortDirection = null)
    : PagedMessage(Page, PageSize), ISortMessage;
