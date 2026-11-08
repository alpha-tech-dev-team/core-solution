using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets;

public sealed record GetSecretsMessage(int Page, int PageSize) : IRequest<Result<SecretListPage>>;
