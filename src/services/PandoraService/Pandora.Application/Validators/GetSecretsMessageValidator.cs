using CoreSolution.Shared.Application.Validation;
using FluentValidation;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Application.Features.Secrets.GetSecrets;
using Pandora.Domain.Enums;

namespace Pandora.Application.Validators;

public sealed class GetSecretsMessageValidator : AbstractValidator<GetSecretsMessage>
{
    public GetSecretsMessageValidator()
    {
        this.ApplyPaginationRules();
        this.ApplySortRules(
            SecretSorting.Instance.SupportedFields,
            nameof(ErrorCode.InvalidSortField),
            nameof(ErrorCode.InvalidSortDirection));
    }
}
