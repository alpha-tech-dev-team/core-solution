using System.Text.Json.Serialization;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Application.Contracts;

namespace Pandora.Application.Features.Permissions;

public sealed class GrantSecretAccessMessage : IRequest<Result>, IAdminUserRequest, IItemIdRequest
{
    [JsonIgnore]
    public Guid AdminUserId { get; private set; }
    [JsonIgnore]
    public string ItemId { get; set; } = string.Empty;
    public long TelegramUserId { get; set; }

    Guid IAdminUserRequest.AdminUserId
    {
        get => AdminUserId;
        set => AdminUserId = value;
    }
}
