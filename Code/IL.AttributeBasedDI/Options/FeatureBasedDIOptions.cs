namespace IL.AttributeBasedDI.Options;

public class FeatureBasedDIOptions<TFeatureFlag> where TFeatureFlag : struct, Enum
{
    public const string ActiveFeaturesAppsettingsPath = "DIFeatureFlags";
    public TFeatureFlag ActiveFeatures { get; set; }

    public void SetActiveFeaturesFromNames(IEnumerable<string> featureNames)
    {
        var activeFeaturesValue = 0;
        foreach (var name in featureNames)
        {
            if (Enum.TryParse<TFeatureFlag>(name, out var feature))
            {
                activeFeaturesValue |= Convert.ToInt32(feature);
            }
        }

        ActiveFeatures = (TFeatureFlag)(object)activeFeaturesValue;
    }
}