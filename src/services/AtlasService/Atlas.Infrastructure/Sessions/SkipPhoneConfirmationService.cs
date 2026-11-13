using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Enums;
using CoreSolution.Shared.Models;

namespace Atlas.Infrastructure.Sessions;

public sealed class SkipPhoneConfirmationService : IPhoneConfirmationService
{
    private readonly ISessionService _sessionService;

    public SkipPhoneConfirmationService(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public async Task<PhoneConfirmationResult> ConfirmAsync(long telegramUserId, string? phone, string initialStep, CancellationToken cancellationToken)
    {
        await _sessionService.SaveSession(UserSession.Create(telegramUserId, initialStep), cancellationToken);

        return PhoneConfirmationResult.Confirmed;
    }
}
