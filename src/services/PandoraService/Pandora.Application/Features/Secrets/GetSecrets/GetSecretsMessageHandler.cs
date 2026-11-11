using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets.GetSecrets;

public sealed class GetSecretsMessageHandler : IRequestHandler<GetSecretsMessage, Result<SecretListPage>>
{
    private readonly ISecretMetadataRepository _metadataRepository;

    public GetSecretsMessageHandler(ISecretMetadataRepository metadataRepository)
    {
        _metadataRepository = metadataRepository;
    }

    public async Task<Result<SecretListPage>> Handle(GetSecretsMessage request, CancellationToken cancellationToken)
    {
        var page = await _metadataRepository.GetPage(
            request.Page,
            request.PageSize,
            OrderByResourceName,
            cancellationToken);

        return Result<SecretListPage>.Success(page);
    }

    private static IOrderedQueryable<SecretMetadata> OrderByResourceName(IQueryable<SecretMetadata> query)
    {
        return query.OrderBy(metadata => metadata.ResourceName).ThenBy(metadata => metadata.VaultItemId);
    }
}
