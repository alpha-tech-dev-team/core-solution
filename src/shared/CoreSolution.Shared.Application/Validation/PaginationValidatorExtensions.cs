using CoreSolution.Shared.Pagination;
using FluentValidation;

namespace CoreSolution.Shared.Application.Validation;

public static class PaginationValidatorExtensions
{
    public static void ApplyPaginationRules<T>(this AbstractValidator<T> validator)
        where T : PagedMessage
    {
        validator.RuleFor(message => message.Page)
            .GreaterThanOrEqualTo(PaginationLimits.DefaultPage)
            .When(message => message.Page.HasValue);

        validator.RuleFor(message => message.PageSize)
            .InclusiveBetween(PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize)
            .When(message => message.PageSize.HasValue);
    }
}
