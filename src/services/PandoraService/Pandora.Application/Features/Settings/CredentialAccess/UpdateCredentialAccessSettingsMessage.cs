using System.Text.Json.Serialization;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Application.Contracts;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Settings.CredentialAccess;

public sealed class UpdateCredentialAccessSettingsMessage
    : IRequest<Result<CredentialAccessSettings>>, IAdminUserRequest
{
    [JsonIgnore]
    public Guid AdminUserId { get; private set; }

    public int TtlMinutes { get; set; }

    Guid IAdminUserRequest.AdminUserId
    {
        get => AdminUserId;
        set => AdminUserId = value;
    }
}
