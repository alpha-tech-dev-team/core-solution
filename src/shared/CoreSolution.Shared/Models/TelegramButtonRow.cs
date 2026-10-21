namespace CoreSolution.Shared.Models;

public sealed class TelegramButtonRow
{
    public TelegramButtonRow(params TelegramButton[] buttons)
    {
        Buttons = buttons;
    }

    public IReadOnlyList<TelegramButton> Buttons { get; }
}
