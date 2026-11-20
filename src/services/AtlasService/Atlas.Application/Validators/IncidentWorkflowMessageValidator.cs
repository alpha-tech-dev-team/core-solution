using Atlas.Application.Features.Incidents;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class IncidentWorkflowMessageValidator : AbstractValidator<IncidentWorkflowMessage>
{
    public IncidentWorkflowMessageValidator()
    {
        RuleFor(message => message.TelegramUserId)
            .GreaterThan(ValidationConstants.TelegramUserIdMinExclusive)
            .WithErrorCode(nameof(ErrorCode.InvalidTelegramUserId));

        RuleFor(message => message.MessageId)
            .GreaterThan(ValidationConstants.MessageIdMinExclusive)
            .WithErrorCode(nameof(ErrorCode.InvalidMessageId));

        RuleFor(message => message.Text)
            .MaximumLength(ValidationConstants.IncidentDescriptionMaxLength)
            .WithErrorCode(nameof(ErrorCode.InvalidIncidentDescription))
            .When(message => message.Text is not null);
    }
}
