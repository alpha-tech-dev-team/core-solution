using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Permissions;

public sealed class GetEmployeeSecretsMessageHandler : IRequestHandler<GetEmployeeSecretsMessage, Result<EmployeeSecretPage>>
{
    private readonly IPermissionRepository _permissionRepository;

    public GetEmployeeSecretsMessageHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<Result<EmployeeSecretPage>> Handle(GetEmployeeSecretsMessage request, CancellationToken cancellationToken)
    {
        var page = await _permissionRepository.GetPage(
            request.TelegramUserId,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result<EmployeeSecretPage>.Success(page);
    }
}
