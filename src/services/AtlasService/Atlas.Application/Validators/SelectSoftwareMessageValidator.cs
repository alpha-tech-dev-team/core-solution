using Atlas.Application.Features.Software;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class SelectSoftwareMessageValidator : AbstractValidator<SelectSoftwareMessage>
{
    public SelectSoftwareMessageValidator()
    {
        RuleFor(message => message.TelegramUserId).GreaterThan(ValidationConstants.TelegramUserIdMinExclusive).WithErrorCode(nameof(ErrorCode.InvalidTelegramUserId));
        RuleFor(message => message.MessageId).GreaterThan(ValidationConstants.MessageIdMinExclusive).WithErrorCode(nameof(ErrorCode.InvalidMessageId));
        RuleFor(message => message.SoftwareId).NotEqual(Guid.Empty).WithErrorCode(nameof(ErrorCode.InvalidSoftwareId));
    }
}
