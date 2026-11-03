namespace CoreSolution.Shared.Models;

public sealed class WorkflowButtonRow
{
    public IReadOnlyList<WorkflowButton> Buttons { get; }
    
    public WorkflowButtonRow(params WorkflowButton[] buttons)
    {
        Buttons = buttons;
    }
}
