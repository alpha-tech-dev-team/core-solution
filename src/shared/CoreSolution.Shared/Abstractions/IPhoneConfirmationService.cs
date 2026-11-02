using CoreSolution.Shared.Enums;

namespace CoreSolution.Shared.Abstractions;

public interface IPhoneConfirmationService
{
    Task<bool> ConfirmAsync(long telegramUserId, string? phone, WorkflowStep initialStep, CancellationToken cancellationToken);
}
