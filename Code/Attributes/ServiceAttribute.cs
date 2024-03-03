namespace IL.AttributeBasedDI.Attributes;

/// <summary>
/// Attribute for reflection based class detection and registration in Microsoft DI container.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ServiceAttribute : DependencyInjectionAttributeBase
{
    /// <summary>
    /// Service Attribute constructor
    /// </summary>
    /// <param name="serviceType">Specifies which service is target for DI registration.
    /// If left null/default service will be automatically retrieved either from first interface current class implements or the class itself will become a serviceType.</param>
    /// <param name="lifetime">Specifies service lifetime.</param>
    public ServiceAttribute(Type? serviceType = null, Lifetime lifetime = Lifetime.Transient)
    {
        ServiceType = serviceType;
        Lifetime = lifetime;
    }

    public Lifetime Lifetime { get; init; }
}