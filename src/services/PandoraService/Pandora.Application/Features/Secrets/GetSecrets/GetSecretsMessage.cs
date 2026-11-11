using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets.GetSecrets;

public sealed class GetSecretsMessage : IRequest<Result<SecretListPage>>
{
    public int? Page { get; set; }

    public int? PageSize { get; set; }
}
