using Atlas.Application.Features.Rooms;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class SelectRoomByTextMessageValidator : AbstractValidator<SelectRoomByTextMessage>
{
    public SelectRoomByTextMessageValidator()
    {
        RuleFor(message => message.TelegramUserId)
            .GreaterThan(ValidationConstants.TelegramUserIdMinExclusive)
            .WithErrorCode(nameof(ErrorCode.InvalidTelegramUserId));

        RuleFor(message => message.MessageId)
            .GreaterThan(ValidationConstants.MessageIdMinExclusive)
            .WithErrorCode(nameof(ErrorCode.InvalidMessageId));

        RuleFor(message => message.Text)
            .NotEmpty()
            .WithErrorCode(nameof(ErrorCode.InvalidRoomText));
    }
}
