namespace CoreSolution.Shared.Models;

public sealed class TelegramMessageContent
{
    public string Text { get; set; } = string.Empty;

    public IReadOnlyList<TelegramButtonRow>? ButtonRows { get; set; }
}
