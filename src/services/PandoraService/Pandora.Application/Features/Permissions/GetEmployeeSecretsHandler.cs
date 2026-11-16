using CoreSolution.Shared.Pagination;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Permissions;

public sealed class GetEmployeeSecretsHandler : IRequestHandler<GetEmployeeSecretsMessage, Result<EmployeeSecretPage>>
{
    private readonly IPermissionRepository _permissionRepository;

    public GetEmployeeSecretsHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<Result<EmployeeSecretPage>> Handle(
        GetEmployeeSecretsMessage request,
        CancellationToken cancellationToken)
    {
        var result = await _permissionRepository.GetPage(
            request.TelegramUserId,
            new SecretPageQuery(request.ToPageRequest(), SecretSorting.ByResourceName),
            cancellationToken);

        return Result<EmployeeSecretPage>.Success(result);
    }
}
