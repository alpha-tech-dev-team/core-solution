using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Models;
using FluentValidation;

namespace CoreSolution.Shared.Application.Validation;

public abstract class PagedQueryValidator<T> : AbstractValidator<T> where T : PagedQuery
{
    protected PagedQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(PaginationLimits.FirstPage)
            .When(query => query.Page.HasValue);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize)
            .When(query => query.PageSize.HasValue);
    }
}
