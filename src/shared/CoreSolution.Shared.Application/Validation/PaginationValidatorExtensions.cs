using CoreSolution.Shared.Pagination;
using FluentValidation;

namespace CoreSolution.Shared.Application.Validation;

public static class PaginationValidatorExtensions
{
    public static void ApplyPaginationRules<T>(this AbstractValidator<T> validator)
        where T : IPagedRequest
    {
        validator.RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(PaginationLimits.DefaultPage)
            .When(request => request.Page.HasValue);

        validator.RuleFor(request => request.PageSize)
            .InclusiveBetween(PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize)
            .When(request => request.PageSize.HasValue);
    }
}
