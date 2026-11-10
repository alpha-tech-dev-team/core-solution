using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Pandora.Domain.Constants;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets;

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

        var deleteResult = await _secretWriter.Delete(request.ItemId, cancellationToken);

        var isOrphaned = deleteResult is SecretDeleteResult.NotFound;

        if (deleteResult is SecretDeleteResult.GatewayError)
        {
            _logger.LogWarning("Шлюз Vaultwarden отклонил удаление секрета {ItemId}", request.ItemId);

            return Result.Failure(nameof(ErrorCode.VaultwardenUnavailable));
        }

        try
        {
            await _metadataRepository.Delete(request.ItemId, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось удалить секрет {ItemId} из базы данных, в Vaultwarden он уже удалён", request.ItemId);

            throw;
        }

        if (isOrphaned)
        {
            _logger.LogWarning("Элемент {ItemId} уже отсутствует в Vaultwarden, устаревшая запись метаданных очищена", request.ItemId);
        }

        await _auditLogger.LogActionAsync(request.AdminUserId, AuditConstants.ActionSecretDeleted, request.ItemId, cancellationToken);

        return Result.Success();
    }
}
