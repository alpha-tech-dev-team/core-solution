using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets;

public sealed record UpdateSecretCommand(Guid AdminUserId, string ItemId, string Name, string Username, string? Password)
    : IRequest<Result<SecretListItem>>;
