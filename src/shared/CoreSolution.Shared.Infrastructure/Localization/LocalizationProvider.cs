using System.Reflection;
using System.Collections.Frozen;
using System.Text.Json;
using CoreSolution.Shared.Enums;

namespace CoreSolution.Shared.Infrastructure.Localization;

public sealed class LocalizationProvider
{
    private const string ResourceNameFormat = "{0}.Localization.Resources.{1}.json";

    public FrozenDictionary<Language, FrozenDictionary<string, FrozenDictionary<string, string>>> Dictionary { get; }

    public LocalizationProvider(Assembly resourceAssembly)
    {
        var assemblyName = resourceAssembly.GetName().Name;
        var mainBuilder = new Dictionary<Language, FrozenDictionary<string, FrozenDictionary<string, string>>>();

        foreach (var language in Enum.GetValues<Language>())
        {
            var languageCode = language.ToString().ToLowerInvariant();
            var resourceName = string.Format(ResourceNameFormat, assemblyName, languageCode);

            using var stream = resourceAssembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                throw new FileNotFoundException(
                    $"Localization resource '{resourceName}' was not embedded in {assemblyName}.",
                    resourceName);
            }

            using var reader = new StreamReader(stream);
            var jsonContent = reader.ReadToEnd();
            var dict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonContent);
            if (dict is null)
            {
                throw new InvalidDataException($"Localization resource '{resourceName}' is empty or invalid.");
            }

            var stateBuilder = new Dictionary<string, FrozenDictionary<string, string>>();

            foreach (var stateEntry in dict)
            {
                var phrasesBuilder = new Dictionary<string, string>();

                foreach (var phrase in stateEntry.Value)
                {
                    phrasesBuilder.TryAdd(phrase.Key, phrase.Value);
                }

                stateBuilder.TryAdd(stateEntry.Key, phrasesBuilder.ToFrozenDictionary());
            }

            mainBuilder.TryAdd(language, stateBuilder.ToFrozenDictionary());
        }

        Dictionary = mainBuilder.ToFrozenDictionary();
    }
}
