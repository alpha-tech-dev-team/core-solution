using System.Text.Json.Serialization;
using CoreSolution.Shared.Pagination;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Application.Contracts;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets.GetSecrets;

public sealed class GetSecretsMessage : IRequest<Result<SecretListPage>>, IPagedRequest, IAdminUserRequest
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public bool? IncludePasswords { get; set; }

    [JsonIgnore]
    public Guid AdminUserId { get; private set; }

    Guid IAdminUserRequest.AdminUserId
    {
        get => AdminUserId;
        set => AdminUserId = value;
    }
}
