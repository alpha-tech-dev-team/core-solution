using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Options;
using Microsoft.Extensions.Options;

namespace CoreSolution.Shared.Infrastructure.Sessions;

public sealed class PhoneConfirmationService : IPhoneConfirmationService
{
    private const string InternationalPrefix = "+";

    private readonly IIdentityClient _identityClient;
    private readonly ISessionService _sessionService;
    private readonly SessionOptions _options;

    public PhoneConfirmationService(
        IIdentityClient identityClient,
        ISessionService sessionService,
        IOptions<SessionOptions> options)
    {
        _identityClient = identityClient;
        _sessionService = sessionService;
        _options = options.Value;
    }

    public bool RequiresPhone => !_options.SkipPhoneConfirmation;

    public async Task<bool> ConfirmAsync(long telegramUserId, string? phone, string initialStep, CancellationToken cancellationToken)
    {
        if (!_options.SkipPhoneConfirmation && phone is not null)
        {
            var approved = await _identityClient.ActivateUser(telegramUserId, NormalizePhone(phone), cancellationToken);

            if (!approved)
            {
                return false;
            }
        }

        await _sessionService.SaveSession(UserSession.Create(telegramUserId, initialStep), cancellationToken);

        return true;
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        return $"{InternationalPrefix}{digits}";
    }
}
