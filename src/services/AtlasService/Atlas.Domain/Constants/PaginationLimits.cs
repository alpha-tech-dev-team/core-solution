using SharedPaginationLimits = CoreSolution.Shared.Constants.PaginationLimits;

namespace Atlas.Domain.Constants;

public static class PaginationLimits
{
    public const int FirstPage = SharedPaginationLimits.FirstPage;
    public const int PageStep = 1;
    public const int DefaultPage = SharedPaginationLimits.DefaultPage;
    public const int DefaultPageSize = SharedPaginationLimits.DefaultPageSize;
    public const int RoomsPageSize = 5;
    public const int MinPageSize = SharedPaginationLimits.MinPageSize;
    public const int MaxPageSize = SharedPaginationLimits.MaxPageSize;
}
