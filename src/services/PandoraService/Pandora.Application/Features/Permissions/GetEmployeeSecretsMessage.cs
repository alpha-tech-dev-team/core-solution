using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Permissions;

public sealed record GetEmployeeSecretsMessage(long TelegramUserId, int Page, int PageSize)
    : IRequest<Result<EmployeeSecretPage>>;
