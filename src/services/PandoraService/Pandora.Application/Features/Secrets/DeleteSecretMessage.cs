using CoreSolution.Shared.Results;
using MediatR;

namespace Pandora.Application.Features.Secrets;

public sealed record DeleteSecretMessage(Guid AdminUserId, string ItemId) : IRequest<Result>;
