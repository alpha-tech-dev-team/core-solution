using CoreSolution.Shared.Models;

namespace Identity.Domain.Models;

public sealed record EmployeeFilter(
    string? Search,
    string? SortBy,
    SortDirection SortDirection,
    bool IncludeArchived);
