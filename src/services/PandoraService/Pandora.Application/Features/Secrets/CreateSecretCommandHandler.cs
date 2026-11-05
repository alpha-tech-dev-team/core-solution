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
    private readonly IAuditRepository _auditRepository;
    private readonly ILogger<CreateSecretCommandHandler> _logger;

    public CreateSecretCommandHandler(
        ISecretWriter secretWriter,
        ISecretMetadataRepository metadataRepository,
        IAuditRepository auditRepository,
        ILogger<CreateSecretCommandHandler> logger)
    {
        _secretWriter = secretWriter;
        _metadataRepository = metadataRepository;
        _auditRepository = auditRepository;
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

            await _secretWriter.Delete(created.Id, CancellationToken.None);

            return Result<SecretListItem>.Failure(nameof(ErrorCode.DatabaseUnavailable));
        }

        await AddAuditSafely(request.AdminUserId, AuditConstants.ActionSecretCreated, created.Id, cancellationToken);

        return Result<SecretListItem>.Success(new SecretListItem(created.Id, name, username));
    }

    private async Task AddAuditSafely(Guid adminUserId, string action, string targetItemId, CancellationToken cancellationToken)
    {
        try
        {
            await _auditRepository.Add(
                new AuditEntry
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    UserId = adminUserId,
                    Action = action,
                    TargetItemId = targetItemId,
                    RequestSource = AuditConstants.SourceWeb
                },
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось записать аудит действия {Action} по секрету {ItemId}", action, targetItemId);
        }
    }
}
