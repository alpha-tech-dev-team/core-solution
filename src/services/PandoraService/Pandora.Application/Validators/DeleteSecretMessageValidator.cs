using FluentValidation;
using Pandora.Application.Features.Secrets.DeleteSecret;

namespace Pandora.Application.Validators;

public class DeleteSecretMessageValidator : AbstractValidator<DeleteSecretMessage>
{
    public DeleteSecretMessageValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty();
    }
}
