namespace CoreSolution.Shared.Models;

public abstract class PagedQuery
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
