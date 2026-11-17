namespace CoreSolution.Shared.Models;

public abstract record PagedMessage(int? Page = null, int? PageSize = null);
