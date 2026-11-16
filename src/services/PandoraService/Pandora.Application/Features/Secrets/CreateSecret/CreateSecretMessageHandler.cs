using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets.CreateSecret;

public sealed class CreateSecretMessageHandler : IRequestHandler<CreateSecretMessage, Result<SecretListItem>>
{
    private readonly ISecretWriter _secretWriter;
    private readonly ISecretMetadataRepository _metadataRepository;
    private readonly SecretAuditLogger _auditLogger;
    private readonly ILogger<CreateSecretMessageHandler> _logger;

    public CreateSecretMessageHandler(
        ISecretWriter secretWriter,
        ISecretMetadataRepository metadataRepository,
        SecretAuditLogger auditLogger,
        ILogger<CreateSecretMessageHandler> logger)
    {
        _secretWriter = secretWriter;
        _metadataRepository = metadataRepository;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task<Result<SecretListItem>> Handle(CreateSecretMessage request, CancellationToken cancellationToken)
    {
        var secretName = request.Normalize();

        var created = await _secretWriter.Create(secretName.Name, secretName.Username, request.Password, cancellationToken);

        if (created is null)
        {
            _logger.LogWarning("Шлюз Vaultwarden отклонил создание секрета {Name}", secretName.Name);

            return Result<SecretListItem>.Failure(nameof(ErrorCode.VaultwardenUnavailable));
        }

        try
        {
            var metadata = new SecretMetadata(created.Id, secretName.Name, secretName.Username);

            await _auditLogger.LogActionAsync(request.AdminUserId, AuditConstants.ActionSecretCreated, created.Id, cancellationToken);

            await _metadataRepository.Add(metadata, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось сохранить метаданные секрета {ItemId}, откатываю создание в Vaultwarden", created.Id);

            await CompensateCreationAsync(created.Id);

            throw;
        }

        return Result<SecretListItem>.Success(new SecretListItem(created.Id, secretName.Name, secretName.Username));
    }

    private async Task CompensateCreationAsync(string itemId)
    {
        try
        {
            var deleteResult = await _secretWriter.Delete(itemId, CancellationToken.None);

            if (deleteResult is SecretDeleteResult.GatewayError)
            {
                _logger.LogError("Не удалось удалить секрет {ItemId} из Vaultwarden после отката создания", itemId);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось удалить секрет {ItemId} из Vaultwarden после отката создания", itemId);
        }
    }
}
