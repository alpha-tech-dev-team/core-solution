using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets;

public sealed class CreateSecretCommandHandler : IRequestHandler<CreateSecretCommand, Result<SecretListItem>>
{
    private readonly ISecretWriter _secretWriter;
    private readonly ISecretMetadataRepository _metadataRepository;
    private readonly SecretAuditLogger _auditLogger;
    private readonly ILogger<CreateSecretCommandHandler> _logger;

    public CreateSecretCommandHandler(
        ISecretWriter secretWriter,
        ISecretMetadataRepository metadataRepository,
        SecretAuditLogger auditLogger,
        ILogger<CreateSecretCommandHandler> logger)
    {
        _secretWriter = secretWriter;
        _metadataRepository = metadataRepository;
        _auditLogger = auditLogger;
        _logger = logger;
    }

    public async Task<Result<SecretListItem>> Handle(CreateSecretCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var username = request.Username.Trim();

        var created = await _secretWriter.Create(name, username, request.Password, cancellationToken);

        if (created is null)
        {
            _logger.LogWarning("Шлюз Vaultwarden отклонил создание секрета {Name}", name);

            return Result<SecretListItem>.Failure(nameof(ErrorCode.VaultwardenUnavailable));
        }

        try
        {
            var metadata = new SecretMetadata
            {
                VaultItemId = created.Id,
                ResourceName = name,
                Username = username
            };

            await _metadataRepository.Add(metadata, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось сохранить метаданные секрета {ItemId}, откатываю создание в Vaultwarden", created.Id);

            await CompensateCreationAsync(created.Id);

            return Result<SecretListItem>.Failure(nameof(ErrorCode.DatabaseUnavailable));
        }

        await _auditLogger.LogActionAsync(request.AdminUserId, AuditConstants.ActionSecretCreated, created.Id, cancellationToken);

        return Result<SecretListItem>.Success(new SecretListItem(created.Id, name, username));
    }

    private async Task CompensateCreationAsync(string itemId)
    {
        try
        {
            var deleted = await _secretWriter.Delete(itemId, CancellationToken.None);

            if (!deleted)
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
