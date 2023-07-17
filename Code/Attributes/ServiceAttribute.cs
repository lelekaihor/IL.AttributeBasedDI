namespace IL.AttributeBasedDI.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ServiceAttribute : Attribute
{
    public ServiceAttribute(Type? serviceType = null, Lifetime lifetime = Lifetime.Transient)
    {
        ServiceType = serviceType;
        Lifetime = lifetime;
    }

    public Lifetime Lifetime { get; set; }
    public Type? ServiceType { get; set; }

    public bool FindServiceTypeAutomatically => ServiceType is null;
}

public enum Lifetime
{
    Transient,
    Scoped,
    Singleton
}