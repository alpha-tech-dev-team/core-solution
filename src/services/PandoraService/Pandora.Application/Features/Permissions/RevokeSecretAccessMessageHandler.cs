using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Domain.Constants;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;

namespace Pandora.Application.Features.Permissions;

public sealed class RevokeSecretAccessMessageHandler : IRequestHandler<RevokeSecretAccessMessage, Result>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly SecretAuditLogger _auditLogger;
    private readonly ILogger<RevokeSecretAccessMessageHandler> _logger;

    public RevokeSecretAccessMessageHandler(
        IPermissionRepository permissionRepository,
        SecretAuditLogger auditLogger,
        ILogger<RevokeSecretAccessMessageHandler> logger)
    {
        _permissionRepository = permissionRepository;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task<Result> Handle(RevokeSecretAccessMessage request, CancellationToken cancellationToken)
    {
        var revoked = await _permissionRepository.Revoke(request.TelegramUserId, request.ItemId, cancellationToken);

        if (!revoked)
        {
            _logger.LogWarning("Отзыв доступа отклонен: секрет {ItemId} не найден", request.ItemId);

            return Result.Failure(nameof(ErrorCode.SecretNotFound));
        }

        _logger.LogInformation("Доступ к секрету {ItemId} отозван у сотрудника", request.ItemId);

        await _auditLogger.LogActionAsync(
            request.AdminUserId,
            AuditConstants.ActionSecretAccessRevoked,
            request.ItemId,
            cancellationToken);

        await _permissionRepository.SaveChanges(cancellationToken);

        return Result.Success();
    }
}
