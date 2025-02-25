namespace IL.AttributeBasedDI.Attributes;

/// <summary>
/// Attribute for reflection based class detection and registration in Microsoft DI container.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
#if NET7_0_OR_GREATER
public class ServiceAttribute : DependencyInjectionAttributeBase
#else
public sealed class ServiceAttribute : DependencyInjectionAttributeBase
#endif
{

#if NET8_0_OR_GREATER
    /// <summary>
    /// Service Attribute constructor
    /// </summary>
    /// <param name="serviceType">Specifies which service is target for DI registration.
    /// If left null/default service will be automatically retrieved either from first interface current class implements or the class itself will become a serviceType.</param>
    /// <param name="lifetime">Specifies service lifetime.</param>
    /// <param name="key">Specifies key which current service will be accessible for as KeyedService from IKeyedServiceProvider</param>
    public ServiceAttribute(Type? serviceType = null, Lifetime lifetime = Lifetime.Transient, string? key = null)
    {
        ServiceType = serviceType;
        Lifetime = lifetime;
        Key = key;
    }
#else
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
#endif

    public Lifetime Lifetime { get; init; }

#if NET8_0_OR_GREATER
    public string? Key { get; init; }
#endif
}