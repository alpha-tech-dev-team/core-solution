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
        var name = request.Name.Trim();
        var username = request.Username.Trim();

        var metadata = await _metadataRepository.GetById(request.ItemId, cancellationToken);

        if (metadata is null)
        {
            return Result<SecretListItem>.Failure(nameof(ErrorCode.SecretNotFound));
        }

        var updateResult = await _secretWriter.Update(
            request.ItemId,
            new SecretUpdateData(name, username, request.Password),
            cancellationToken);

        if (updateResult is SecretUpdateResult.NotFound)
        {
            return Result<SecretListItem>.Failure(nameof(ErrorCode.SecretNotFound));
        }

        if (updateResult is SecretUpdateResult.GatewayError)
        {
            _logger.LogWarning("Шлюз Vaultwarden отклонил обновление секрета {ItemId}", request.ItemId);

            return Result<SecretListItem>.Failure(nameof(ErrorCode.VaultwardenUnavailable));
        }

        if (updateResult is SecretUpdateResult.NoPassword)
        {
            return Result<SecretListItem>.Failure(nameof(ErrorCode.InvalidSecretData));
        }

        try
        {
            metadata.Rename(name, username);

            await _metadataRepository.Update(metadata, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось сохранить изменения секрета {ItemId} в базе данных, в Vaultwarden уже применены новые значения", request.ItemId);

            throw;
        }

        await _auditLogger.LogActionAsync(request.AdminUserId, AuditConstants.ActionSecretUpdated, request.ItemId, cancellationToken);

        return Result<SecretListItem>.Success(new SecretListItem(request.ItemId, name, username));
    }
}
