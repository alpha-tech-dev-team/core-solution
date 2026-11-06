using CoreSolution.Shared.Results;
using MediatR;

namespace Pandora.Application.Features.Secrets;

public sealed record DeleteSecretCommand(Guid AdminUserId, string ItemId) : IRequest<Result>;
