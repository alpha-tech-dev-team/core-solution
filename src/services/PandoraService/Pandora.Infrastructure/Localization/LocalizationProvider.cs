using System.Collections.Frozen;
using System.Text.Json;
using Pandora.Core.Constants.Localization;
using Pandora.Core.Enums;

namespace Pandora.Infrastructure.Localization;

public class LocalizationProvider
{
    public FrozenDictionary<Language, FrozenDictionary<WorkflowStep, FrozenDictionary<string, string>>> Dictionary { get; }

    public LocalizationProvider()
    {
        var mainBuilder = new Dictionary<Language, FrozenDictionary<WorkflowStep, FrozenDictionary<string, string>>>();
        var unknownSections = new List<string>();

        foreach (var language in Enum.GetValues<Language>())
        {
            var languageCode = language.ToString().ToLowerInvariant();
            var filePath = Path.Combine(AppContext.BaseDirectory, LocalizationConstants.LocalizationPath, LocalizationConstants.ResourcesPath, $"{languageCode}{LocalizationConstants.ResourceFileExtension}");

            if (File.Exists(filePath))
            {
                var jsonContent = File.ReadAllText(filePath);
                var dict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonContent);

                if (dict is not null)
                {
                    var stateBuilder = new Dictionary<WorkflowStep, FrozenDictionary<string, string>>();

                    foreach (var stateEntry in dict)
                    {
                        if (!Enum.TryParse<WorkflowStep>(stateEntry.Key, true, out var state))
                        {
                            unknownSections.Add($"{languageCode}/{stateEntry.Key}");
                            continue;
                        }

                        var phrasesBuilder = new Dictionary<string, string>();

                        foreach (var phrase in stateEntry.Value)
                        {
                            phrasesBuilder.TryAdd(phrase.Key, phrase.Value);
                        }

                        stateBuilder.TryAdd(state, phrasesBuilder.ToFrozenDictionary());
                    }

                    mainBuilder.TryAdd(language, stateBuilder.ToFrozenDictionary());
                }
            }
        }

        Dictionary = mainBuilder.ToFrozenDictionary();

        if (unknownSections.Count > 0)
        {
            throw new InvalidOperationException(
                "Unknown localization sections (must match a WorkflowStep value): "
                + string.Join(", ", unknownSections));
        }
    }
}
