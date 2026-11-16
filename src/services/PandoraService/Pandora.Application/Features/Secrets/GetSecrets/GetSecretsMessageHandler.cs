using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Enums;
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

        if (ResolveSource(request.Source) is SecretSource.Vaultwarden)
        {
            return await ReadVaultPageAsync(page, pageSize, cancellationToken);
        }

        var dbPage = await _metadataRepository.GetPage(
            new SecretPageQuery(page, pageSize, OrderByResourceName),
            cancellationToken);

        return Result<SecretListPage>.Success(dbPage);
    }

    private async Task<Result<SecretListPage>> ReadVaultPageAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var itemsResult = await _vaultReader.GetListItems(cancellationToken);

        if (itemsResult.Value is null)
        {
            return Result<SecretListPage>.Failure(itemsResult.ErrorCode ?? string.Empty);
        }

        var items = itemsResult.Value;
        var pageItems = items
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Id)
            .Skip((page - PaginationLimits.FirstPage) * pageSize)
            .Take(pageSize)
            .ToList();

        return Result<SecretListPage>.Success(new SecretListPage(pageItems, page, pageSize, items.Count));
    }

    private static SecretSource ResolveSource(string? source)
    {
        return Enum.TryParse<SecretSource>(source, ignoreCase: true, out var parsed) ? parsed : SecretSource.Database;
    }

    private static IOrderedQueryable<SecretMetadata> OrderByResourceName(IQueryable<SecretMetadata> query)
    {
        return query.OrderBy(metadata => metadata.ResourceName).ThenBy(metadata => metadata.VaultItemId);
    }
}
