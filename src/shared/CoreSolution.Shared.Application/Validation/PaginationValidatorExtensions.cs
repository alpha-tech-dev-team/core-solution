using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Models;
using FluentValidation;

namespace CoreSolution.Shared.Application.Validation;

public static class PaginationValidatorExtensions
{
    public static void ApplyPaginationRules<T>(
        this AbstractValidator<T> validator,
        string? pageErrorCode = null,
        string? pageSizeErrorCode = null)
        where T : PagedMessage
    {
        var pageRule = validator.RuleFor(message => message.Page)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .GreaterThanOrEqualTo(PaginationLimits.FirstPage);

        if (pageErrorCode is not null)
        {
            pageRule.WithErrorCode(pageErrorCode);
        }

        var pageSizeRule = validator.RuleFor(message => message.PageSize)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .InclusiveBetween(PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize);

        if (pageSizeErrorCode is not null)
        {
            pageSizeRule.WithErrorCode(pageSizeErrorCode);
        }
    }
}
