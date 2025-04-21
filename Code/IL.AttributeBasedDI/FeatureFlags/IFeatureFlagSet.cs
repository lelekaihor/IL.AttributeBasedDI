namespace IL.AttributeBasedDI.FeatureFlags;

public interface IFeatureFlagSet
{
    bool IsFeatureActive<TFeatureFlag>(TFeatureFlag feature) where TFeatureFlag : struct, Enum;
}