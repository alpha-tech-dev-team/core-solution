using Atlas.Domain.Entities;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Notifications;

public record NotifyIncidentResolvedMessage(Incident Incident) : IRequest<Result>;
