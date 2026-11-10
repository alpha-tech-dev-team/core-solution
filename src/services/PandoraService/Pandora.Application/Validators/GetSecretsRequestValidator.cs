using FluentValidation;
using Pandora.Application.Contracts;
using Pandora.Domain.Constants;

namespace Pandora.Application.Validators;

public class GetSecretsRequestValidator : AbstractValidator<GetSecretsRequest>
{
    public GetSecretsRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(PaginationLimits.DefaultPage)
            .When(x => x.Page.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize)
            .When(x => x.PageSize.HasValue);
    }
}
