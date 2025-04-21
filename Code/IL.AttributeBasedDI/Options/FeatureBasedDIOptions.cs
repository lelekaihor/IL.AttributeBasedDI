using IL.AttributeBasedDI.FeatureFlags;
using Microsoft.Extensions.Configuration;

namespace IL.AttributeBasedDI.Options;

// ReSharper disable once InconsistentNaming
public class FeatureBasedDIOptions(IConfiguration configuration)
{
    private const string FeatureFlagsAppSettingsPath = "DIFeatureFlags";

    public FeatureFlagSet ActiveFeatures { get; } = new();

    public bool ThrowWhenDecorationTypeNotFound { get; set; }

    /// <summary>
    /// Adds or merges a feature enum instance into the active feature set.
    /// </summary>
    public void AddFeature(Enum featureEnum)
    {
        ActiveFeatures.AddOrMerge(featureEnum);
    }

    /// <summary>
    /// Adds feature flags by their names from config into the active feature set.
    /// </summary>
    public void SetFeaturesFromConfig(Dictionary<string, Type> enumTypesByConfigKey, string featureFlagsAppSettingsPath = FeatureFlagsAppSettingsPath)
    {
        foreach (var (key, type) in enumTypesByConfigKey)
        {
            var section = configuration.GetSection($"{featureFlagsAppSettingsPath}:{key}");
            var names = section.Get<string[]>();
            if (names == null) continue;

            if (!type.IsEnum || !type.IsDefined(typeof(FlagsAttribute), false))
                throw new InvalidOperationException($"{type.Name} must be a [Flags] enum");

            long combined = 0;
            foreach (var name in names)
            {
                if (Enum.TryParse(type, name, ignoreCase: true, out var value))
                {
                    combined |= Convert.ToInt64(value);
                }
            }

            var combinedEnum = (Enum)Enum.ToObject(type, combined);
            ActiveFeatures.AddOrMerge(combinedEnum);
        }
    }

    public bool IsFeatureActive<TFeatureFlag>(TFeatureFlag feature) where TFeatureFlag : struct, Enum
        => ActiveFeatures.IsFeatureActive(feature);
}