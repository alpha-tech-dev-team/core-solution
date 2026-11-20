using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets.GetSecrets;

public sealed record GetSecretsMessage : PagedMessage, IRequest<Result<SecretListPage>>;
