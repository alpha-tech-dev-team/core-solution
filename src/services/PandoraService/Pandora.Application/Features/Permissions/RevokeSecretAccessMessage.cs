using CoreSolution.Shared.Results;
using MediatR;

namespace Pandora.Application.Features.Permissions;

public sealed record RevokeSecretAccessMessage(Guid AdminUserId, long TelegramUserId, string ItemId) : IRequest<Result>;
