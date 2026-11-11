using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets.CreateSecret;

public sealed record CreateSecretMessage(Guid AdminUserId, string Name, string Username, string Password)
    : IRequest<Result<SecretListItem>>;
