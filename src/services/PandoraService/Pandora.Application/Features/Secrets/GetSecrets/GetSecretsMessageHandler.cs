using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets.GetSecrets;

public sealed class GetSecretsMessageHandler : IRequestHandler<GetSecretsMessage, Result<SecretListPage>>
{
    private readonly ISecretMetadataRepository _metadataRepository;
    private readonly ISecretVaultReader _vaultReader;

    public GetSecretsMessageHandler(ISecretMetadataRepository metadataRepository, ISecretVaultReader vaultReader)
    {
        _metadataRepository = metadataRepository;
        _vaultReader = vaultReader;
    }

    public async Task<Result<SecretListPage>> Handle(GetSecretsMessage request, CancellationToken cancellationToken)
    {
        var page = request.Page ?? PaginationLimits.DefaultPage;
        var pageSize = request.PageSize ?? PaginationLimits.DefaultPageSize;

        var dbPage = await _metadataRepository.GetPage(
            new SecretPageQuery(page, pageSize, OrderByResourceName),
            cancellationToken);

        var items = dbPage.Items
            .Select(item => new SecretPageItem(item.Id, item.Name, item.Username, null))
            .ToList();

        if (request.IncludePasswords is not true)
        {
            return Result<SecretListPage>.Success(new SecretListPage(items, dbPage.Page, dbPage.PageSize, dbPage.Total));
        }

        var passwordsResult = await _vaultReader.GetListItems(cancellationToken);

        if (passwordsResult.Value is null)
        {
            return Result<SecretListPage>.Failure(passwordsResult.ErrorCode ?? string.Empty);
        }

        var passwordsById = passwordsResult.Value.ToDictionary(item => item.Id, item => item.Password);

        var withPasswords = items
            .Select(item => passwordsById.TryGetValue(item.Id, out var password)
                ? item with { Password = password }
                : item)
            .ToList();

        return Result<SecretListPage>.Success(new SecretListPage(withPasswords, dbPage.Page, dbPage.PageSize, dbPage.Total));
    }

    private static IOrderedQueryable<SecretMetadata> OrderByResourceName(IQueryable<SecretMetadata> query)
    {
        return query.OrderBy(metadata => metadata.ResourceName).ThenBy(metadata => metadata.VaultItemId);
    }
}
