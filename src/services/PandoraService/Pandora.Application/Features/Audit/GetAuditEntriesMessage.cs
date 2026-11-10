using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Audit;

public sealed record GetAuditEntriesMessage(int Page, int PageSize) : IRequest<Result<AuditEntryPage>>;
