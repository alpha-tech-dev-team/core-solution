using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Pandora.Domain.Constants;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;

namespace Pandora.Application.Features.Secrets;

public sealed class DeleteSecretCommandHandler : IRequestHandler<DeleteSecretCommand, Result>
{
    private readonly ISecretWriter _secretWriter;
    private readonly ISecretMetadataRepository _metadataRepository;
    private readonly SecretAuditLogger _auditLogger;
    private readonly ILogger<DeleteSecretCommandHandler> _logger;

    public DeleteSecretCommandHandler(
        ISecretWriter secretWriter,
        ISecretMetadataRepository metadataRepository,
        SecretAuditLogger auditLogger,
        ILogger<DeleteSecretCommandHandler> logger)
    {
        _secretWriter = secretWriter;
        _metadataRepository = metadataRepository;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteSecretCommand request, CancellationToken cancellationToken)
    {
        var metadata = await _metadataRepository.Get(request.ItemId, cancellationToken);

        if (metadata is null)
        {
            return Result.Failure(nameof(ErrorCode.SecretNotFound));
        }

        if (!await _secretWriter.Delete(request.ItemId, cancellationToken))
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

            return Result.Failure(nameof(ErrorCode.DatabaseUnavailable));
        }

        await _auditLogger.LogActionAsync(request.AdminUserId, AuditConstants.ActionSecretDeleted, request.ItemId, cancellationToken);

        return Result.Success();
    }
}
