namespace CoreSolution.Shared.Models;

public sealed class WorkflowButton
{
    public WorkflowButton(string text, string callbackData)
    {
        Text = text;
        CallbackData = callbackData;
    }

    public string Text { get; }
    public string CallbackData { get; }
}
