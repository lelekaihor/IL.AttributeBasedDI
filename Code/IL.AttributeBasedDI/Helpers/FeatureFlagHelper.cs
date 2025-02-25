namespace IL.AttributeBasedDI.Helpers;

public static class FeatureFlagHelper
{
    public static bool IsFeatureEnabled<TFeatureFlag>(TFeatureFlag activeFeatures, TFeatureFlag feature) where TFeatureFlag : struct, Enum
    {
        return activeFeatures.HasFlag(feature);
    }
}