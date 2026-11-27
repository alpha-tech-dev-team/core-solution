using CoreSolution.Shared.Models;
using Identity.Application.Constants;

namespace Identity.Application.Contracts.Admin;

public record GetUsersRequest(
    int? Page = ValidationConstants.PageMin,
    int? PageSize = ValidationConstants.PageSizeDefault,
    bool IncludeArchived = false,
    string? Search = null,
    string? SortBy = null,
    SortDirection? SortDirection = null)
    : PagedSearchSortMessage(Page, PageSize, Search, SortBy, SortDirection);
