using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets;

public sealed record GetSecretsQuery : IRequest<Result<IReadOnlyList<SecretListItem>>>;
