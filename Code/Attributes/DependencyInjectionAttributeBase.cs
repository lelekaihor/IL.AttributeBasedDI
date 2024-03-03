namespace IL.AttributeBasedDI.Attributes;

public abstract class DependencyInjectionAttributeBase : Attribute
{
    public Type? ServiceType { get; init; }

    public bool FindServiceTypeAutomatically => ServiceType is null;
}