using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Domain.Constants;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Settings.CredentialAccess;

public sealed class UpdateCredentialAccessSettingsHandler
    : IRequestHandler<UpdateCredentialAccessSettingsMessage, Result<CredentialAccessSettings>>
{
    private readonly ICredentialAccessSettingsRepository _settingsRepository;
    private readonly SecretAuditLogger _auditLogger;

    public UpdateCredentialAccessSettingsHandler(
        ICredentialAccessSettingsRepository settingsRepository,
        SecretAuditLogger auditLogger)
    {
        _settingsRepository = settingsRepository;
        _auditLogger = auditLogger;
    }

    public async Task<Result<CredentialAccessSettings>> Handle(
        UpdateCredentialAccessSettingsMessage request,
        CancellationToken cancellationToken)
    {
        await _auditLogger.LogActionAsync(
            request.AdminUserId,
            AuditConstants.ActionCredentialAccessSettingsUpdated,
            null,
            cancellationToken);

        var settings = await _settingsRepository.Update(
            request.TtlMinutes,
            request.AdminUserId,
            cancellationToken);

        return Result<CredentialAccessSettings>.Success(settings);
    }
}
