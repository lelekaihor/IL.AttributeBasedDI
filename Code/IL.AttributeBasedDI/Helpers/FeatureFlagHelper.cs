using IL.AttributeBasedDI.Models;

namespace IL.AttributeBasedDI.Helpers;

public static class FeatureFlagHelper
{
    public static bool IsFeatureEnabled<TFeatureFlag>(TFeatureFlag activeFeatures, TFeatureFlag feature) where TFeatureFlag : struct, Enum
    {
        var featureValue = Convert.ToInt32(feature);

        return feature is FeaturesNoop ||
               featureValue != 0 // 0 stands for None
               && activeFeatures.HasFlag(feature);
    }
}