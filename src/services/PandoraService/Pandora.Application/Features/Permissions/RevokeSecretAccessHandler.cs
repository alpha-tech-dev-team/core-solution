using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Domain.Constants;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Permissions;

public sealed class RevokeSecretAccessHandler : IRequestHandler<RevokeSecretAccessMessage, Result>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly SecretAuditLogger _auditLogger;
    private readonly ILogger<RevokeSecretAccessHandler> _logger;

    public RevokeSecretAccessHandler(
        IPermissionRepository permissionRepository,
        SecretAuditLogger auditLogger,
        ILogger<RevokeSecretAccessHandler> logger)
    {
        _permissionRepository = permissionRepository;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task<Result> Handle(RevokeSecretAccessMessage request, CancellationToken cancellationToken)
    {
        var result = await _permissionRepository.Revoke(
            request.TelegramUserId,
            request.ItemId,
            cancellationToken);

        if (result is SecretAccessRevokeResult.SecretNotFound)
        {
            _logger.LogWarning("Отзыв доступа отклонён: секрет {ItemId} не найден", request.ItemId);

            return Result.Failure(nameof(ErrorCode.SecretNotFound));
        }

        if (result is SecretAccessRevokeResult.AlreadyRevoked)
        {
            _logger.LogInformation("Доступ к секрету {ItemId} уже отсутствует", request.ItemId);

            return Result.Success();
        }

        _logger.LogInformation("Доступ к секрету {ItemId} отозван", request.ItemId);

        await _auditLogger.LogActionAsync(
            request.AdminUserId,
            AuditConstants.ActionSecretAccessRevoked,
            request.ItemId,
            request.TelegramUserId,
            cancellationToken);

        await _permissionRepository.SaveChanges(cancellationToken);

        return Result.Success();
    }
}
