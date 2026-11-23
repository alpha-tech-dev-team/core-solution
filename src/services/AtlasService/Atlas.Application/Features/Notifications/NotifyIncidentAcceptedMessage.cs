using Atlas.Domain.Entities;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Notifications;

public record NotifyIncidentAcceptedMessage(Incident Incident) : IRequest<Result>;
