using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Domain.Constants;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets.UpdateSecret;

public sealed class UpdateSecretMessageHandler : IRequestHandler<UpdateSecretMessage, Result<SecretListItem>>
{
    private readonly ISecretWriter _secretWriter;
    private readonly ISecretMetadataRepository _metadataRepository;
    private readonly SecretAuditLogger _auditLogger;
    private readonly ILogger<UpdateSecretMessageHandler> _logger;

    public UpdateSecretMessageHandler(
        ISecretWriter secretWriter,
        ISecretMetadataRepository metadataRepository,
        SecretAuditLogger auditLogger,
        ILogger<UpdateSecretMessageHandler> logger)
    {
        _secretWriter = secretWriter;
        _metadataRepository = metadataRepository;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task<Result<SecretListItem>> Handle(UpdateSecretMessage request, CancellationToken cancellationToken)
    {
        var secretName = request.Normalize();

        var metadata = await _metadataRepository.GetById(request.ItemId, cancellationToken);

        if (metadata is null)
        {
            return Result<SecretListItem>.Failure(nameof(ErrorCode.SecretNotFound));
        }

        metadata.Rename(secretName.Name, secretName.Username);
        metadata.MarkPending(SecretSyncOperation.Update);

        await _auditLogger.LogActionAsync(request.AdminUserId, AuditConstants.ActionSecretUpdated, request.ItemId, cancellationToken);

        await _metadataRepository.Update(metadata, cancellationToken);

        var updateResult = await _secretWriter.Update(
            request.ItemId,
            new SecretUpdateData(secretName.Name, secretName.Username, request.Password),
            cancellationToken);

        if (updateResult is not SecretUpdateResult.Success)
        {
            return FailureFor(updateResult, request.ItemId);
        }

        await _metadataRepository.ClearSyncStateIfUnchanged(request.ItemId, secretName.Name, secretName.Username, cancellationToken);

        return Result<SecretListItem>.Success(new SecretListItem(request.ItemId, secretName.Name, secretName.Username));
    }

    private Result<SecretListItem> FailureFor(SecretUpdateResult updateResult, string itemId)
    {
        switch (updateResult)
        {
            case SecretUpdateResult.NotFound:
                return Result<SecretListItem>.Failure(nameof(ErrorCode.SecretNotFound));
            case SecretUpdateResult.NoPassword:
                return Result<SecretListItem>.Failure(nameof(ErrorCode.InvalidSecretData));
            default:
                _logger.LogWarning("Шлюз Vaultwarden отклонил обновление секрета {ItemId}, ожидает повторной синхронизации", itemId);

                return Result<SecretListItem>.Failure(nameof(ErrorCode.VaultwardenUnavailable));
        }
    }
}
