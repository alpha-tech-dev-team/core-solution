using Atlas.Application.Common;
using Atlas.Domain.Constants.Localization;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;

namespace Atlas.Application.Features.Incidents;

public sealed class IncidentConfirmationBuilder
{
    private readonly IIncidentCategoryRepository _categoryRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IHardwareRepository _hardwareRepository;
    private readonly ISoftwareRepository _softwareRepository;
    private readonly ILocalizer _localizer;

    public IncidentConfirmationBuilder(
        IIncidentCategoryRepository categoryRepository,
        IRoomRepository roomRepository,
        IHardwareRepository hardwareRepository,
        ISoftwareRepository softwareRepository,
        ILocalizer localizer)
    {
        _categoryRepository = categoryRepository;
        _roomRepository = roomRepository;
        _hardwareRepository = hardwareRepository;
        _softwareRepository = softwareRepository;
        _localizer = localizer;
    }

    public async Task<WorkflowResponse> BuildAsync(AtlasUserSession session, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(session.IncidentCategoryId!.Value, cancellationToken);
        var room = await _roomRepository.GetByIdAsync(session.RoomId!.Value, cancellationToken);
        var hardware = session.HardwareId is Guid hardwareId
            ? await _hardwareRepository.GetByIdAsync(hardwareId, cancellationToken)
            : null;
        var software = session.SoftwareId is Guid softwareId
            ? await _softwareRepository.GetByIdAsync(softwareId, cancellationToken)
            : null;

        var assetLine = hardware is not null
            ? string.Format(GetPhrase(LocalizationKeysConstants.IncidentConfirmation.HardwareLine), hardware.Name)
            : software is not null
                ? string.Format(GetPhrase(LocalizationKeysConstants.IncidentConfirmation.SoftwareLine), software.Name)
                : string.Empty;

        return new WorkflowResponse
        {
            Text = string.Format(
                GetPhrase(LocalizationKeysConstants.IncidentConfirmation.Text),
                category?.Name ?? string.Empty,
                room?.Name ?? string.Empty,
                assetLine,
                session.IncidentDescription),
            ButtonRows = new List<WorkflowButtonRow>
            {
                new(
                    new WorkflowButton(
                        GetPhrase(LocalizationKeysConstants.IncidentConfirmation.Confirm),
                        CallbackConstants.IncidentPrefix + IncidentStepAction.Confirm),
                    new WorkflowButton(
                        GetPhrase(LocalizationKeysConstants.IncidentConfirmation.Cancel),
                        CallbackConstants.IncidentPrefix + IncidentStepAction.Cancel))
            }
        };
    }

    private string GetPhrase(string key)
    {
        return _localizer.GetPhrase(nameof(WorkflowStep.IncidentConfirmation), key);
    }
}
