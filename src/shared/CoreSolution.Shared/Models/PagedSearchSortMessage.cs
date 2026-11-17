namespace CoreSolution.Shared.Models;

public abstract record PagedSearchSortMessage(
    int? Page = null,
    int? PageSize = null,
    string? Search = null,
    string? SortBy = null,
    SortDirection? SortDirection = null)
    : PagedMessage(Page, PageSize), ISortMessage;
