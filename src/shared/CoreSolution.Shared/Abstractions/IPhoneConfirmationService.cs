namespace CoreSolution.Shared.Abstractions;

public interface IPhoneConfirmationService
{
    bool RequiresPhone { get; }

    Task<bool> ConfirmAsync(long telegramUserId, string? phone, string initialStep, CancellationToken cancellationToken);
}
