using System.Collections.Frozen;
using System.Text.Json;
using Atlas.Core.Constants.Localization;
using Atlas.Core.Enums;

namespace Atlas.Infrastructure.Localization;

public class LocalizationProvider
{
    public FrozenDictionary<Language, FrozenDictionary<WorkflowStep, FrozenDictionary<string, string>>> Dictionary { get; }

    public LocalizationProvider()
    {
        var assembly = typeof(LocalizationProvider).Assembly;
        var mainBuilder = new Dictionary<Language, FrozenDictionary<WorkflowStep, FrozenDictionary<string, string>>>();
        var unknownSections = new List<string>();

        foreach (var language in Enum.GetValues<Language>())
        {
            var languageCode = language.ToString().ToLowerInvariant();
            var resourceName = $"{typeof(LocalizationProvider).Namespace}.Resources.{languageCode}.json";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                throw new FileNotFoundException(
                    $"Localization resource '{resourceName}' was not embedded in {assembly.GetName().Name}.",
                    resourceName);
            }

            using var reader = new StreamReader(stream);
            var jsonContent = reader.ReadToEnd();
            var dict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonContent);
            if (dict is null)
            {
                throw new InvalidDataException($"Localization resource '{resourceName}' is empty or invalid.");
            }

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

        Dictionary = mainBuilder.ToFrozenDictionary();

        if (unknownSections.Count > 0)
        {
            throw new InvalidOperationException(
                "Unknown localization sections (must match a WorkflowStep value): "
                + string.Join(", ", unknownSections));
        }
    }
}
