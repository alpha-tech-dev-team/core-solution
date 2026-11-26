using Atlas.Application.Common;
using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Menu;

public sealed class ShowMenuHandler : IRequestHandler<ShowMenuMessage, Result<WorkflowResponse>>
{
    private readonly IIncidentCategoryRepository _categoryRepository;
    private readonly IRequestContext _requestContext;
    private readonly ISessionService _sessionService;
    private readonly ILocalizer _localizer;

    public ShowMenuHandler(
        IIncidentCategoryRepository categoryRepository,
        IRequestContext requestContext,
        ISessionService sessionService,
        ILocalizer localizer)
    {
        _categoryRepository = categoryRepository;
        _requestContext = requestContext;
        _sessionService = sessionService;
        _localizer = localizer;
    }

    public async Task<Result<WorkflowResponse>> Handle(ShowMenuMessage request, CancellationToken cancellationToken)
    {
        var page = await _categoryRepository.GetPageAsync(
            new PageRequest(request.EffectivePage, request.EffectivePageSize),
            IncidentCategorySorting.Apply,
            cancellationToken);

        if (page.Items.Count == 0)
        {
            return Result<WorkflowResponse>.Failure(nameof(ErrorCode.IncidentCategoriesNotFound));
        }

        if (_requestContext.Session is AtlasUserSession session)
        {
            session.RoomId = null;
            session.HardwareId = null;
            session.SoftwareId = null;

            await _sessionService.UpdateState(
                session,
                nameof(WorkflowStep.ProblemCategorySelection),
                cancellationToken);
        }
        else if (await _sessionService.GetSession(request.TelegramUserId, cancellationToken) is AtlasUserSession stored)
        {
            stored.RoomId = null;
            stored.HardwareId = null;
            stored.SoftwareId = null;

            await _sessionService.UpdateState(
                stored,
                nameof(WorkflowStep.ProblemCategorySelection),
                cancellationToken);
        }

        var buttonRows = page.Items
            .Select(category => new WorkflowButtonRow(
                new WorkflowButton(category.Name, CallbackConstants.CategoryPrefix + category.Id)))
            .ToList();

        if (page.HasNextPage)
        {
            buttonRows.Add(new WorkflowButtonRow(
                new WorkflowButton(
                    _localizer.GetPhrase(
                        nameof(WorkflowStep.ProblemCategorySelection),
                        LocalizationKeysConstants.ProblemCategorySelection.NextPage),
                    CallbackConstants.CategoryPagePrefix + page.NextPage)));
        }

        return Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = _localizer.GetPhrase(
                nameof(WorkflowStep.ProblemCategorySelection),
                LocalizationKeysConstants.ProblemCategorySelection.Text),
            ButtonRows = buttonRows
        });
    }
}
