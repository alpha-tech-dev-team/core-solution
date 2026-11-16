using FluentValidation;
using Pandora.Application.Features.Secrets.GetSecrets;
using Pandora.Domain.Constants;
using Pandora.Domain.Enums;

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

        RuleFor(x => x.Source)
            .Must(BeKnownSource)
            .When(x => !string.IsNullOrWhiteSpace(x.Source));
    }

    private static bool BeKnownSource(string? source)
    {
        return Enum.TryParse<SecretSource>(source, ignoreCase: true, out _);
    }
}
