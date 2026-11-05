using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets;

public sealed class GetSecretsQueryHandler : IRequestHandler<GetSecretsQuery, Result<IReadOnlyList<SecretListItem>>>
{
    private readonly ISecretMetadataRepository _metadataRepository;

    public GetSecretsQueryHandler(ISecretMetadataRepository metadataRepository)
    {
        _metadataRepository = metadataRepository;
    }

    public async Task<Result<IReadOnlyList<SecretListItem>>> Handle(GetSecretsQuery request, CancellationToken cancellationToken)
    {
        var items = await _metadataRepository.GetItems(cancellationToken);

        return Result<IReadOnlyList<SecretListItem>>.Success(items);
    }
}
