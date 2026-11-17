using FluentValidation;
using Pandora.Application.Features.Secrets.GetSecrets;
using Pandora.Application.Validation;

namespace Pandora.Application.Validators;

public sealed class GetSecretsMessageValidator : AbstractValidator<GetSecretsMessage>
{
    public GetSecretsMessageValidator()
    {
        this.AddPagingRules();
    }
}
