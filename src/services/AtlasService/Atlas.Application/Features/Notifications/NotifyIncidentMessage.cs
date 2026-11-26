using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Notifications;

public record NotifyIncidentMessage(Incident Incident, IncidentNotificationType Type) : IRequest<Result>;
