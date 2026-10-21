namespace Pandora.App.Features.Start;

public sealed record StartResponse(string Text, IReadOnlyList<string> Services);
