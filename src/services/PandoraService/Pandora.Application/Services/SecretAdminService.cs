using CoreSolution.Shared.Results;
using Microsoft.Extensions.Logging;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Interfaces.Services;
using Pandora.Domain.Models;

namespace Pandora.Application.Services;

public sealed class SecretAdminService : ISecretAdminService
{
    private readonly ISecretWriter _secretWriter;
    private readonly ISecretMetadataRepository _metadataRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ILogger<SecretAdminService> _logger;

    public SecretAdminService(
        ISecretWriter secretWriter,
        ISecretMetadataRepository metadataRepository,
        IAuditRepository auditRepository,
        ILogger<SecretAdminService> logger)
    {
        _secretWriter = secretWriter;
        _metadataRepository = metadataRepository;
        _auditRepository = auditRepository;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<SecretListItem>>> GetItems(CancellationToken cancellationToken)
    {
        var items = await _metadataRepository.GetItems(cancellationToken);

        return Result<IReadOnlyList<SecretListItem>>.Success(items);
    }

    public async Task<Result<SecretListItem>> Create(Guid adminUserId, string name, string username, string password, CancellationToken cancellationToken)
    {
        name = name.Trim();
        username = username.Trim();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(password))
        {
            return Result<SecretListItem>.Failure(nameof(ErrorCode.InvalidSecretData));
        }

        if (name.Length > SecretMetadataLimits.ResourceNameMaxLength || username.Length > SecretMetadataLimits.UsernameMaxLength)
        {
            return Result<SecretListItem>.Failure(nameof(ErrorCode.InvalidSecretData));
        }

        var created = await _secretWriter.Create(name, username, password, cancellationToken);

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

        await AddAuditSafely(adminUserId, AuditConstants.ActionSecretCreated, created.Id, cancellationToken);

        return Result<SecretListItem>.Success(new SecretListItem(created.Id, name, username));
    }

    public async Task<Result<SecretListItem>> Update(Guid adminUserId, string itemId, string name, string username, string? password, CancellationToken cancellationToken)
    {
        name = name.Trim();
        username = username.Trim();

        if (string.IsNullOrWhiteSpace(itemId) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(username))
        {
            return Result<SecretListItem>.Failure(nameof(ErrorCode.InvalidSecretData));
        }

        if (name.Length > SecretMetadataLimits.ResourceNameMaxLength || username.Length > SecretMetadataLimits.UsernameMaxLength)
        {
            return Result<SecretListItem>.Failure(nameof(ErrorCode.InvalidSecretData));
        }

        var metadata = await _metadataRepository.Get(itemId, cancellationToken);

        if (metadata is null)
        {
            return Result<SecretListItem>.Failure(nameof(ErrorCode.SecretNotFound));
        }

        if (!await _secretWriter.Update(itemId, name, username, password, cancellationToken))
        {
            _logger.LogWarning("Шлюз Vaultwarden отклонил обновление секрета {ItemId}", itemId);

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
            _logger.LogError(exception, "Не удалось сохранить изменения секрета {ItemId} в базе данных, в Vaultwarden уже применены новые значения", itemId);

            return Result<SecretListItem>.Failure(nameof(ErrorCode.DatabaseUnavailable));
        }

        await AddAuditSafely(adminUserId, AuditConstants.ActionSecretUpdated, itemId, cancellationToken);

        return Result<SecretListItem>.Success(new SecretListItem(itemId, name, username));
    }

    public async Task<Result> Delete(Guid adminUserId, string itemId, CancellationToken cancellationToken)
    {
        var metadata = await _metadataRepository.Get(itemId, cancellationToken);

        if (metadata is null)
        {
            return Result.Failure(nameof(ErrorCode.SecretNotFound));
        }

        if (!await _secretWriter.Delete(itemId, cancellationToken))
        {
            _logger.LogWarning("Шлюз Vaultwarden отклонил удаление секрета {ItemId}", itemId);

            return Result.Failure(nameof(ErrorCode.VaultwardenUnavailable));
        }

        try
        {
            await _metadataRepository.Delete(itemId, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось удалить секрет {ItemId} из базы данных, в Vaultwarden он уже удалён", itemId);

            return Result.Failure(nameof(ErrorCode.DatabaseUnavailable));
        }

        await AddAuditSafely(adminUserId, AuditConstants.ActionSecretDeleted, itemId, cancellationToken);

        return Result.Success();
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
