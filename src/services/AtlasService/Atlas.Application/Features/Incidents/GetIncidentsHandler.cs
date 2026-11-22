using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Incidents;

public sealed class GetIncidentsHandler : IRequestHandler<GetIncidentsMessage, Result<IncidentListPage>>
{
    private readonly IIncidentRepository _incidentRepository;

    public GetIncidentsHandler(IIncidentRepository incidentRepository)
    {
        _incidentRepository = incidentRepository;
    }

    public async Task<Result<IncidentListPage>> Handle(
        GetIncidentsMessage request,
        CancellationToken cancellationToken)
    {
        var page = await _incidentRepository.GetListPageAsync(
            new IncidentPageQuery(request, IncidentSorting.ByNewest),
            cancellationToken);

        return Result<IncidentListPage>.Success(page);
    }
}
