using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Settings.CredentialAccess;

public sealed class GetCredentialAccessSettingsMessage : IRequest<Result<CredentialAccessSettings>>
{
}
