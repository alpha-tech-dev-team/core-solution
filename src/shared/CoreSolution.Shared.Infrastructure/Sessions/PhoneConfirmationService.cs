using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Models;

namespace CoreSolution.Shared.Infrastructure.Sessions;

public sealed class PhoneConfirmationService : IPhoneConfirmationService
{
    private readonly IIdentityClient _identityClient;
    private readonly ISessionService _sessionService;

    public PhoneConfirmationService(IIdentityClient identityClient, ISessionService sessionService)
    {
        _identityClient = identityClient;
        _sessionService = sessionService;
    }

    public async Task<string?> ConfirmAsync(long telegramUserId, string? phone, string initialStep, CancellationToken cancellationToken)
    {
        if (phone is not null)
        {
            var error = await _identityClient.ActivateUser(telegramUserId, PhoneNumbers.Normalize(phone), cancellationToken);

            if (error is not null)
            {
                return error;
            }
        }

        await _sessionService.SaveSession(UserSession.Create(telegramUserId, initialStep), cancellationToken);

        return null;
    }
}
