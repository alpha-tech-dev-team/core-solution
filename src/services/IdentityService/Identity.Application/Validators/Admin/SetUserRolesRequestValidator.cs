using CoreSolution.Shared.Enums;
using FluentValidation;
using Identity.Application.Constants;
using Identity.Application.Contracts.Admin;
using Identity.Application.Errors;

namespace Identity.Application.Validators.Admin;

public class SetUserRolesRequestValidator : AbstractValidator<SetUserRolesRequest>
{
    public SetUserRolesRequestValidator()
    {
        RuleFor(x => x.Roles)
            .NotNull();

        RuleForEach(x => x.Roles)
            .IsInEnum();

        RuleFor(x => x.AdminLogin)
            .NotEmpty()
            .WithErrorCode(nameof(IdentityErrorCode.AdminCredentialsRequired))
            .MinimumLength(ValidationConstants.AdminUsernameMinLength)
            .MaximumLength(ValidationConstants.AdminUsernameMaxLength)
            .When(RequiresAdminCredentials);

        RuleFor(x => x.AdminPassword)
            .NotEmpty()
            .WithErrorCode(nameof(IdentityErrorCode.AdminCredentialsRequired))
            .MinimumLength(ValidationConstants.PasswordMinLength)
            .MaximumLength(ValidationConstants.PasswordMaxLength)
            .When(RequiresAdminCredentials);
    }

    private static bool RequiresAdminCredentials(SetUserRolesRequest request)
    {
        return request.Roles is not null
            && request.Roles.Any(role => role is UserRole.AtlasAdmin or UserRole.PandoraAdmin);
    }
}
