namespace CoreSolution.Shared.Models;

public sealed class WorkflowButton
{
    public string Text { get; }
    public string CallbackData { get; }
    
    public WorkflowButton(string text, string callbackData)
    {
        Text = text;
        CallbackData = callbackData;
    }
}
