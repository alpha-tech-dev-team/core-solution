using Atlas.Application.Common;
using Atlas.Application.Features.Rooms;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.IncidentCategories;

public sealed class SelectProblemCategoryHandler
    : SessionStateHandler<SelectProblemCategoryMessage>,
      IRequestHandler<SelectProblemCategoryMessage, Result<WorkflowResponse>>
{
    private readonly IIncidentCategoryRepository _categoryRepository;
    private readonly IMediator _mediator;

    protected override string NextWorkflowState => nameof(WorkflowStep.RoomSelection);

    public SelectProblemCategoryHandler(
        IIncidentCategoryRepository categoryRepository,
        IRequestContext requestContext,
        ISessionService sessionService,
        IMediator mediator)
        : base(requestContext, sessionService)
    {
        _categoryRepository = categoryRepository;
        _mediator = mediator;
    }

    public async Task<Result<WorkflowResponse>> Handle(
        SelectProblemCategoryMessage request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentCategoryNotFound));
        }

        await UpdateState(request, cancellationToken);

        return await _mediator.Send(
            new ShowRoomsMessage(request.TelegramUserId, request.MessageId, PaginationLimits.FirstPage),
            cancellationToken);
    }

    protected override void ApplyState(AtlasUserSession session, SelectProblemCategoryMessage request)
    {
        session.IncidentCategoryId = request.CategoryId;
    }
}
