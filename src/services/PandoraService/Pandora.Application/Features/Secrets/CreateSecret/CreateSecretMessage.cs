using System.Text.Json.Serialization;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Application.Contracts;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets.CreateSecret;

public sealed class CreateSecretMessage : IRequest<Result<SecretListItem>>, IAdminUserRequest
{
    [JsonIgnore]
    public Guid AdminUserId { get; private set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    Guid IAdminUserRequest.AdminUserId
    {
        get => AdminUserId;
        set => AdminUserId = value;
    }
}
