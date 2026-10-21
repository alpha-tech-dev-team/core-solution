namespace Pandora.App.Common;

public sealed record BotResponse(string Text, IReadOnlyList<string> Buttons);
