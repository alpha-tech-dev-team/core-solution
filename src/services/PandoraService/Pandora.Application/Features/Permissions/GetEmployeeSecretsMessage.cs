using CoreSolution.Shared.Pagination;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Permissions;

public sealed record GetEmployeeSecretsMessage : PagedMessage, IRequest<Result<EmployeeSecretPage>>
{
    public long TelegramUserId { get; init; }
}
