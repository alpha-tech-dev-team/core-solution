using Atlas.Application.Common;
using Atlas.Domain.Constants;
using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Rooms;

public sealed class ShowRoomsHandler
    : SessionStateHandler<ShowRoomsMessage>, IRequestHandler<ShowRoomsMessage, Result<WorkflowResponse>>
{
    private readonly IRoomRepository _roomRepository;
    private readonly ILocalizer _localizer;

    protected override string NextWorkflowState => nameof(WorkflowStep.RoomSelection);

    public ShowRoomsHandler(
        IRoomRepository roomRepository,
        IRequestContext requestContext,
        ISessionService sessionService,
        ILocalizer localizer)
        : base(requestContext, sessionService)
    {
        _roomRepository = roomRepository;
        _localizer = localizer;
    }

    public async Task<Result<WorkflowResponse>> Handle(ShowRoomsMessage request, CancellationToken cancellationToken)
    {
        await UpdateState(request, cancellationToken);

        var page = await _roomRepository.GetPageAsync(
            new PageRequest(request.Page, PaginationLimits.RoomsPageSize),
            RoomSorting.Apply,
            cancellationToken);

        if (page.Items.Count == 0)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.RoomsNotFound));
        }

        var buttonRows = page.Items
            .Select(room => new WorkflowButtonRow(
                new WorkflowButton(room.Name, CallbackConstants.RoomPrefix + room.Id)))
            .ToList();

        if (page.HasNextPage)
        {
            buttonRows.Add(new WorkflowButtonRow(
                new WorkflowButton(
                    _localizer.GetPhrase(
                        nameof(WorkflowStep.RoomSelection),
                        LocalizationKeysConstants.RoomSelection.NextPage),
                    CallbackConstants.RoomPagePrefix + page.NextPage)));
        }

        return Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = _localizer.GetPhrase(
                nameof(WorkflowStep.RoomSelection),
                LocalizationKeysConstants.RoomSelection.Text),
            ButtonRows = buttonRows
        });
    }

    protected override void ApplyState(AtlasUserSession session, ShowRoomsMessage request)
    {
        session.RoomMenuMessageId = request.MessageId;
    }
}
