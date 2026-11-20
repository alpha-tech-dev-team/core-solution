namespace Atlas.Domain.Enums;

public enum WorkflowStep
{
    Idle,
    Menu,
    ProblemCategorySelection,
    CategorySelected,
    RoomSelection,
    HardwareSelection,
    SoftwareSelection,
    IncidentDescription,
    IncidentConfirmation,
    IncidentCreated
}
