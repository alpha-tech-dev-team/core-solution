using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Domain.Constants;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets.DeleteSecret;

public sealed class DeleteSecretMessageHandler : IRequestHandler<DeleteSecretMessage, Result>
{
    private readonly ISecretWriter _secretWriter;
    private readonly ISecretMetadataRepository _metadataRepository;
    private readonly SecretAuditLogger _auditLogger;
    private readonly ILogger<DeleteSecretMessageHandler> _logger;

    public DeleteSecretMessageHandler(
        ISecretWriter secretWriter,
        ISecretMetadataRepository metadataRepository,
        SecretAuditLogger auditLogger,
        ILogger<DeleteSecretMessageHandler> logger)
    {
        _secretWriter = secretWriter;
        _metadataRepository = metadataRepository;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteSecretMessage request, CancellationToken cancellationToken)
    {
        var metadata = await _metadataRepository.GetById(request.ItemId, cancellationToken);

        if (metadata is null)
        {
            return Result.Failure(nameof(ErrorCode.SecretNotFound));
        }

        metadata.MarkPending(SecretSyncOperation.Delete);

        await _auditLogger.LogActionAsync(request.AdminUserId, AuditConstants.ActionSecretDeleted, request.ItemId, cancellationToken);

        await _metadataRepository.Update(metadata, cancellationToken);

        var deleteResult = await _secretWriter.Delete(request.ItemId, cancellationToken);

        if (deleteResult is SecretDeleteResult.GatewayError)
        {
            _logger.LogWarning("Шлюз Vaultwarden отклонил удаление секрета {ItemId}, ожидает повторной синхронизации", request.ItemId);

            return Result.Failure(nameof(ErrorCode.VaultwardenUnavailable));
        }

        await _metadataRepository.Delete(request.ItemId, cancellationToken);

        if (deleteResult is SecretDeleteResult.NotFound)
        {
            _logger.LogWarning("Элемент {ItemId} уже отсутствует в Vaultwarden, устаревшая запись метаданных очищена", request.ItemId);
        }

        return Result.Success();
    }
}
