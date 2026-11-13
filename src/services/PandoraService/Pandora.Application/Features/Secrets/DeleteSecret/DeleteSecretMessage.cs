using System.Text.Json.Serialization;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Application.Contracts;

namespace Pandora.Application.Features.Secrets.DeleteSecret;

public sealed class DeleteSecretMessage : IRequest<Result>, IAdminUserRequest, IItemIdRequest
{
    [JsonIgnore]
    public Guid AdminUserId { get; private set; }

    [JsonIgnore]
    public string ItemId { get; set; } = string.Empty;

    Guid IAdminUserRequest.AdminUserId
    {
        get => AdminUserId;
        set => AdminUserId = value;
    }
}
