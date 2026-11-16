using CoreSolution.Shared.Pagination;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Permissions;

public sealed class GetEmployeeSecretsMessage : IRequest<Result<EmployeeSecretPage>>, IPagedRequest
{
    public long TelegramUserId { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
