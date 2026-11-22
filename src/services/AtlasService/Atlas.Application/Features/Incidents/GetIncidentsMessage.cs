using Atlas.Domain.Models;
using CoreSolution.Shared.Pagination;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Incidents;

public sealed record GetIncidentsMessage : PagedMessage, IRequest<Result<IncidentListPage>>;
