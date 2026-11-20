using System.Text.Json.Serialization;
using Atlas.Domain.Enums;
using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Enums;
using CoreSolution.Shared.Models;

namespace Atlas.Domain.Models;

public sealed class AtlasUserSession : UserSession
{
    public Guid? IncidentCategoryId { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? HardwareId { get; set; }
    public Guid? SoftwareId { get; set; }
    public string? IncidentDescription { get; set; }
    public int? RoomMenuMessageId { get; set; }
    [JsonIgnore]
    public WorkflowStep Step
    {
        get => Enum.TryParse<WorkflowStep>(State, out var step) ? step : WorkflowStep.Idle;
    }

    public AtlasUserSession(long tgUserId, string state, Language language, DateTime lastUpdate)
        : base(tgUserId, state, language, lastUpdate)
    {
    }

    public new static AtlasUserSession Create(long tgUserId, string initialState)
    {
        return new AtlasUserSession(tgUserId, initialState, LocalizationConstants.DefaultLanguage, DateTime.UtcNow);
    }

    public void ResetIncident()
    {
        IncidentCategoryId = null;
        RoomId = null;
        HardwareId = null;
        SoftwareId = null;
        IncidentDescription = null;
    }

    public void ResetIncidentDescription()
    {
        IncidentDescription = null;
    }
}