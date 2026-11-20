using Atlas.Application.Common;
using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Software;

public sealed class SelectSoftwareHandler
    : SessionStateHandler<SelectSoftwareMessage>,
      IRequestHandler<SelectSoftwareMessage, Result<WorkflowResponse>>
{
    private readonly ISoftwareRepository _softwareRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IIncidentCategoryRepository _categoryRepository;
    private readonly ILocalizer _localizer;

    protected override string NextWorkflowState => nameof(WorkflowStep.IncidentDescription);

    public SelectSoftwareHandler(
        ISoftwareRepository softwareRepository,
        IRoomRepository roomRepository,
        IIncidentCategoryRepository categoryRepository,
        IRequestContext requestContext,
        ISessionService sessionService,
        ILocalizer localizer)
        : base(requestContext, sessionService)
    {
        _softwareRepository = softwareRepository;
        _roomRepository = roomRepository;
        _categoryRepository = categoryRepository;
        _localizer = localizer;
    }

    public async Task<Result<WorkflowResponse>> Handle(SelectSoftwareMessage request, CancellationToken cancellationToken)
    {
        if (Session.RoomId is not Guid roomId || Session.IncidentCategoryId is not Guid categoryId)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentContextIncomplete));
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);

        if (category?.Type != IncidentCategoryType.Software)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentContextIncomplete));
        }

        var software = await _softwareRepository.GetByIdAsync(request.SoftwareId, cancellationToken);

        if (software is null)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.SoftwareNotFound));
        }

        var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken);

        await UpdateState(request, cancellationToken);

        return Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = string.Format(
                _localizer.GetPhrase(
                    nameof(WorkflowStep.IncidentDescription),
                    LocalizationKeysConstants.IncidentDescription.Text),
                room?.Name ?? string.Empty,
                software.Version is null ? software.Name : $"{software.Name} ({software.Version})")
        });
    }

    protected override void ApplyState(AtlasUserSession session, SelectSoftwareMessage request)
    {
        session.SoftwareId = request.SoftwareId;
        session.HardwareId = null;
    }
}
