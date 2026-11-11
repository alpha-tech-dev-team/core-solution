using FluentValidation;
using Pandora.Application.Features.Secrets.GetSecrets;
using Pandora.Domain.Constants;

namespace Pandora.Application.Validators;

public class GetSecretsMessageValidator : AbstractValidator<GetSecretsMessage>
{
    public GetSecretsMessageValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(PaginationLimits.DefaultPage)
            .When(x => x.Page.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize)
            .When(x => x.PageSize.HasValue);
    }
}
