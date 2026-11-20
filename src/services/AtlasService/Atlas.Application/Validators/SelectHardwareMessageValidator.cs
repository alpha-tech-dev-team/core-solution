using Atlas.Application.Features.Hardware;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class SelectHardwareMessageValidator : AbstractValidator<SelectHardwareMessage>
{
    public SelectHardwareMessageValidator()
    {
        RuleFor(message => message.TelegramUserId).GreaterThan(ValidationConstants.TelegramUserIdMinExclusive).WithErrorCode(nameof(ErrorCode.InvalidTelegramUserId));
        RuleFor(message => message.MessageId).GreaterThan(ValidationConstants.MessageIdMinExclusive).WithErrorCode(nameof(ErrorCode.InvalidMessageId));
        RuleFor(message => message.HardwareId).NotEqual(Guid.Empty).WithErrorCode(nameof(ErrorCode.InvalidHardwareId));
    }
}
