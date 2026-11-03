namespace CoreSolution.Shared.Models;

public sealed class WorkflowResponse
{
    public string Text { get; set; } = string.Empty;
    public IReadOnlyList<WorkflowButtonRow>? ButtonRows { get; set; }
    public bool RequestContact { get; set; }
    public string? ContactButtonText { get; set; }
}
