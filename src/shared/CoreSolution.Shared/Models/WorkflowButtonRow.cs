namespace CoreSolution.Shared.Models;

public sealed class WorkflowButtonRow
{
    public WorkflowButtonRow(params WorkflowButton[] buttons)
    {
        Buttons = buttons;
    }

    public IReadOnlyList<WorkflowButton> Buttons { get; }
}
