using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets;

public sealed class UpdateSecretCommandHandler : IRequestHandler<UpdateSecretCommand, Result<SecretListItem>>
{
    private readonly ISecretWriter _secretWriter;
    private readonly ISecretMetadataRepository _metadataRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ILogger<UpdateSecretCommandHandler> _logger;

    public UpdateSecretCommandHandler(
        ISecretWriter secretWriter,
        ISecretMetadataRepository metadataRepository,
        IAuditRepository auditRepository,
        ILogger<UpdateSecretCommandHandler> logger)
    {
        _secretWriter = secretWriter;
        _metadataRepository = metadataRepository;
        _auditRepository = auditRepository;
        _logger = logger;
    }

    public async Task<Result<SecretListItem>> Handle(UpdateSecretCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ItemId))
        {
            return Result<SecretListItem>.Failure(nameof(ErrorCode.InvalidSecretData));
        }

        var name = request.Name.Trim();
        var username = request.Username.Trim();

        var metadata = await _metadataRepository.Get(request.ItemId, cancellationToken);

        if (metadata is null)
        {
            return Result<SecretListItem>.Failure(nameof(ErrorCode.SecretNotFound));
        }

        if (!await _secretWriter.Update(request.ItemId, name, username, request.Password, cancellationToken))
        {
            _logger.LogWarning("Шлюз Vaultwarden отклонил обновление секрета {ItemId}", request.ItemId);

            return Result<SecretListItem>.Failure(nameof(ErrorCode.VaultwardenUnavailable));
        }

        try
        {
            metadata.ResourceName = name;
            metadata.Username = username;

            await _metadataRepository.Update(metadata, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось сохранить изменения секрета {ItemId} в базе данных, в Vaultwarden уже применены новые значения", request.ItemId);

            return Result<SecretListItem>.Failure(nameof(ErrorCode.DatabaseUnavailable));
        }

        await AddAuditSafely(request.AdminUserId, AuditConstants.ActionSecretUpdated, request.ItemId, cancellationToken);

        return Result<SecretListItem>.Success(new SecretListItem(request.ItemId, name, username));
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
