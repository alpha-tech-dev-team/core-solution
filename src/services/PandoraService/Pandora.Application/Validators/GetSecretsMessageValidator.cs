using CoreSolution.Shared.Application.Validation;
using FluentValidation;
using Pandora.Application.Features.Secrets.GetSecrets;

namespace Pandora.Application.Validators;

public sealed class GetSecretsMessageValidator : AbstractValidator<GetSecretsMessage>
{
    public GetSecretsMessageValidator()
    {
        this.ApplyPaginationRules();
    }
}
