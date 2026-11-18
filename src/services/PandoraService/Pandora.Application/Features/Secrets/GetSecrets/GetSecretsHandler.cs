using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Domain.Constants;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets.GetSecrets;

public sealed class GetSecretsHandler : IRequestHandler<GetSecretsMessage, Result<SecretListPage>>
{
    private readonly ISecretMetadataRepository _metadataRepository;
    private readonly ISecretVaultReader _vaultReader;
    private readonly SecretAuditLogger _auditLogger;
    private readonly IAuditSaver _auditSaver;
    private readonly ILogger<GetSecretsHandler> _logger;

    public GetSecretsHandler(
        ISecretMetadataRepository metadataRepository,
        ISecretVaultReader vaultReader,
        SecretAuditLogger auditLogger,
        IAuditSaver auditSaver,
        ILogger<GetSecretsHandler> logger)
    {
        _metadataRepository = metadataRepository;
        _vaultReader = vaultReader;
        _auditLogger = auditLogger;
        _auditSaver = auditSaver;
        _logger = logger;
    }

    public async Task<Result<SecretListPage>> Handle(GetSecretsMessage request, CancellationToken cancellationToken)
    {
        var dbPage = await _metadataRepository.GetPage(
            new SecretPageMessage(request, SecretSorting.ByResourceName),
            cancellationToken);

        if (request.IncludePasswords is not true)
        {
            return Result<SecretListPage>.Success(dbPage);
        }

        var passwordsResult = await _vaultReader.GetListItems(cancellationToken);

        if (passwordsResult.Value is null)
        {
            _logger.LogWarning(
                "Пароли не получены из Vaultwarden ({ErrorCode}), список отдан без паролей",
                passwordsResult.ErrorCode);

            return Result<SecretListPage>.Success(dbPage);
        }

        var passwordsById = passwordsResult.Value
            .GroupBy(item => item.Id)
            .ToDictionary(group => group.Key, group => group.First().Password);

        var items = dbPage.Items
            .Select(item => passwordsById.TryGetValue(item.Id, out var password)
                ? item with { Password = password }
                : item)
            .ToList();

        foreach (var item in items.Where(item => item.Password is not null))
        {
            await _auditLogger.LogActionAsync(
                request.AdminUserId,
                AuditConstants.ActionSecretPasswordsViewed,
                item.Id,
                cancellationToken);
        }

        await _auditSaver.SaveChanges(cancellationToken);

        return Result<SecretListPage>.Success(
            new SecretListPage(items, dbPage.Page, dbPage.PageSize, dbPage.Total));
    }
}
