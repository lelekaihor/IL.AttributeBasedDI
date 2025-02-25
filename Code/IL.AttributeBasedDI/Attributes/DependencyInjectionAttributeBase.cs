namespace IL.AttributeBasedDI.Attributes;

public abstract class DependencyInjectionAttributeBase<TFeatureFlag> : Attribute where TFeatureFlag : struct, Enum
{
    public Type? ServiceType { get; init; }

    public bool FindServiceTypeAutomatically => ServiceType is null;

    public TFeatureFlag Feature { get; init; }

    public DependencyInjectionAttributeBase(TFeatureFlag feature)
    {
        Feature = feature;
    }
}