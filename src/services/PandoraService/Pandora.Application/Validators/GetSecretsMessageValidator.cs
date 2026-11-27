using CoreSolution.Shared.Application.Validation;
using FluentValidation;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Application.Features.Secrets.GetSecrets;
using Pandora.Application.Validation;

namespace Pandora.Application.Validators;

public sealed class GetSecretsMessageValidator : AbstractValidator<GetSecretsMessage>
{
    public GetSecretsMessageValidator()
    {
        this.ApplyPaginationRules();
        this.AddSortRules(SecretSorting.Instance.SupportedFields);
    }
}
