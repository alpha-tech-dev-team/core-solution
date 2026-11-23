namespace CoreSolution.Shared.Models;

public interface ISortMessage
{
    string? SortBy { get; }
    SortDirection? SortDirection { get; }
}
