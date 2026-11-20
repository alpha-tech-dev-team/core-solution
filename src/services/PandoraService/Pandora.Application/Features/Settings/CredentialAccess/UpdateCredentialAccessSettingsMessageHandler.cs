using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Constants;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Settings.CredentialAccess;

public sealed class UpdateCredentialAccessSettingsMessageHandler
    : IRequestHandler<UpdateCredentialAccessSettingsMessage, Result<CredentialAccessSettings>>
{
    private readonly ICredentialAccessSettingsRepository _settingsRepository;

    public UpdateCredentialAccessSettingsMessageHandler(ICredentialAccessSettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async Task<Result<CredentialAccessSettings>> Handle(
        UpdateCredentialAccessSettingsMessage request,
        CancellationToken cancellationToken)
    {
        if (!CredentialAccessConstants.AllowedTtlMinutes.Contains(request.TtlMinutes))
        {
            return Result<CredentialAccessSettings>.Failure(nameof(ErrorCode.MalformedRequest));
        }

        var settings = await _settingsRepository.Update(
            request.TtlMinutes,
            request.AdminUserId,
            cancellationToken);

        return Result<CredentialAccessSettings>.Success(settings);
    }
}
