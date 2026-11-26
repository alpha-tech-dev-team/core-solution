using Atlas.Domain.Enums;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Incidents;

public record IncidentWorkflowMessage(
    long TelegramUserId,
    int MessageId,
    IncidentStepAction? Action,
    string? Text = null,
    string? EmployeeName = null,
    string? EmployeeSurname = null)
    : IRequest<Result<WorkflowResponse>>, IAuthorizedBotRequest;
