namespace Pandora.Domain.Models;

public sealed record AuditEntryFilter(
    string? Search,
    string? Action,
    string? RequestSource,
    string? SortBy,
    AuditSortDirection SortDirection);
