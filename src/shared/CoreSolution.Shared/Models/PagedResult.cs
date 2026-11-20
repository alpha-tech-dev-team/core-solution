namespace CoreSolution.Shared.Models;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);
