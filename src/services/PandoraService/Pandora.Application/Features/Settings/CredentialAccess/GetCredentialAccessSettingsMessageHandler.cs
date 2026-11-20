using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Settings.CredentialAccess;

public sealed class GetCredentialAccessSettingsMessageHandler
    : IRequestHandler<GetCredentialAccessSettingsMessage, Result<CredentialAccessSettings>>
{
    private readonly ICredentialAccessSettingsRepository _settingsRepository;

    public GetCredentialAccessSettingsMessageHandler(ICredentialAccessSettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async Task<Result<CredentialAccessSettings>> Handle(
        GetCredentialAccessSettingsMessage request,
        CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.Get(cancellationToken);

        return Result<CredentialAccessSettings>.Success(settings);
    }
}
