using System.Text.Json.Serialization;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Application.Contracts;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets.UpdateSecret;

public sealed class UpdateSecretMessage : IRequest<Result<SecretListItem>>, IAdminUserRequest, IItemIdRequest
{
    [JsonIgnore]
    public Guid AdminUserId { get; private set; }

    [JsonIgnore]
    public string ItemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }

    Guid IAdminUserRequest.AdminUserId
    {
        get => AdminUserId;
        set => AdminUserId = value;
    }
}
