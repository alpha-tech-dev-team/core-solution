using CoreSolution.Shared.Models;

namespace Pandora.Domain.Models;

public sealed record AuditEntryFilter(
    string? Search,
    string? Action,
    string? RequestSource,
    string? SortBy,
    SortDirection SortDirection);
