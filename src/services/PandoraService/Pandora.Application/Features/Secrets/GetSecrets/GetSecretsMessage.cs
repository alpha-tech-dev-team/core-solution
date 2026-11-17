using System.Text.Json.Serialization;
using CoreSolution.Shared.Pagination;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Application.Contracts;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets.GetSecrets;

public sealed record GetSecretsMessage : PagedMessage, IRequest<Result<SecretListPage>>, IAdminUserRequest
{
    public bool? IncludePasswords { get; init; }

    [JsonIgnore]
    public Guid AdminUserId { get; private set; }

    Guid IAdminUserRequest.AdminUserId
    {
        get => AdminUserId;
        set => AdminUserId = value;
    }
}
