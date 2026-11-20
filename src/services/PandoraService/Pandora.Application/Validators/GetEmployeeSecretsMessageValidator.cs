using CoreSolution.Shared.Application.Validation;
using FluentValidation;
using Pandora.Application.Features.Permissions;

namespace Pandora.Application.Validators;

public sealed class GetEmployeeSecretsMessageValidator : AbstractValidator<GetEmployeeSecretsMessage>
{
    public GetEmployeeSecretsMessageValidator()
    {
        this.ApplyPaginationRules();
    }
}
