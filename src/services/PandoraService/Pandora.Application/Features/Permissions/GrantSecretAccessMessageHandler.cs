using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Domain.Constants;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;

namespace Pandora.Application.Features.Permissions;

public sealed class GrantSecretAccessMessageHandler : IRequestHandler<GrantSecretAccessMessage, Result>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly SecretAuditLogger _auditLogger;
    private readonly ILogger<GrantSecretAccessMessageHandler> _logger;

    public GrantSecretAccessMessageHandler(
        IPermissionRepository permissionRepository,
        SecretAuditLogger auditLogger,
        ILogger<GrantSecretAccessMessageHandler> logger)
    {
        _permissionRepository = permissionRepository;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task<Result> Handle(GrantSecretAccessMessage request, CancellationToken cancellationToken)
    {
        var granted = await _permissionRepository.Grant(request.TelegramUserId, request.ItemId, cancellationToken);

        if (!granted)
        {
            _logger.LogWarning("Выдача доступа отклонена: секрет {ItemId} не найден", request.ItemId);

            return Result.Failure(nameof(ErrorCode.SecretNotFound));
        }

        _logger.LogInformation("Доступ к секрету {ItemId} выдан сотруднику", request.ItemId);

        await _auditLogger.LogActionAsync(
            request.AdminUserId,
            AuditConstants.ActionSecretAccessGranted,
            request.ItemId,
            cancellationToken);

        await _permissionRepository.SaveChanges(cancellationToken);

        return Result.Success();
    }
}
