using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Domain.Constants;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Permissions;

public sealed class GrantSecretAccessHandler : IRequestHandler<GrantSecretAccessMessage, Result>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly SecretAuditLogger _auditLogger;
    private readonly ILogger<GrantSecretAccessHandler> _logger;

    public GrantSecretAccessHandler(
        IPermissionRepository permissionRepository,
        SecretAuditLogger auditLogger,
        ILogger<GrantSecretAccessHandler> logger)
    {
        _permissionRepository = permissionRepository;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task<Result> Handle(GrantSecretAccessMessage request, CancellationToken cancellationToken)
    {
        var result = await _permissionRepository.Grant(
            request.TelegramUserId,
            request.ItemId,
            request.AdminUserId,
            cancellationToken);

        if (result is SecretAccessGrantResult.SecretNotFound)
        {
            _logger.LogWarning("Выдача доступа отклонена: секрет {ItemId} не найден", request.ItemId);

            return Result.Failure(nameof(ErrorCode.SecretNotFound));
        }

        if (result is SecretAccessGrantResult.AlreadyGranted)
        {
            _logger.LogInformation("Доступ к секрету {ItemId} уже выдан", request.ItemId);

            return Result.Success();
        }

        _logger.LogInformation("Доступ к секрету {ItemId} выдан", request.ItemId);

        await _auditLogger.LogActionAsync(
            request.AdminUserId,
            AuditConstants.ActionSecretAccessGranted,
            request.ItemId,
            request.TelegramUserId,
            cancellationToken);

        await _permissionRepository.SaveChanges(cancellationToken);

        return Result.Success();
    }
}
