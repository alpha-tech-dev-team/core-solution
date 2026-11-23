using CoreSolution.Shared.Pagination;
using FluentValidation;

namespace CoreSolution.Shared.Application.Validation;

public static class PaginationValidatorExtensions
{
    public static void ApplyPaginationRules<T>(this AbstractValidator<T> validator)
        where T : PagedMessage
    {
        validator.RuleFor(message => message.Page)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .GreaterThanOrEqualTo(PaginationLimits.FirstPage);

        validator.RuleFor(message => message.PageSize)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .InclusiveBetween(PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize);
    }
}
