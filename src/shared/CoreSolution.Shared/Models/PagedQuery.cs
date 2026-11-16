namespace CoreSolution.Shared.Models;

public abstract record PagedQuery
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
