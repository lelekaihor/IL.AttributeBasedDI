using IL.AttributeBasedDI.Models;

namespace IL.AttributeBasedDI.Attributes;

public class ServiceAttribute : ServiceAttribute<FeaturesNoop>
{
    /// <summary>
    /// Service Attribute constructor
    /// </summary>
    /// <param name="serviceType">Specifies which service is target for DI registration.
    /// If left null/default service will be automatically retrieved either from first interface current class implements or the class itself will become a serviceType.</param>
    /// <param name="lifetime">Specifies service lifetime.</param>
    /// <param name="key">Specifies key which current service will be accessible for as KeyedService from IKeyedServiceProvider</param>
    public ServiceAttribute(Type? serviceType = null, Lifetime lifetime = Lifetime.Transient, string? key = null) : base(serviceType, lifetime, key)
    {
        ServiceType = serviceType;
        Lifetime = lifetime;
        Key = key;
    }
}

/// <summary>
/// Attribute for reflection based class detection and registration in Microsoft DI container.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class ServiceAttribute<TFeatureFlag> : DependencyInjectionAttributeBase<TFeatureFlag> where TFeatureFlag : struct, Enum
{
    /// <summary>
    /// Service Attribute constructor
    /// </summary>
    /// <param name="serviceType">Specifies which service is target for DI registration.
    /// If left null/default service will be automatically retrieved either from first interface current class implements or the class itself will become a serviceType.</param>
    /// <param name="lifetime">Specifies service lifetime.</param>
    /// <param name="key">Specifies key which current service will be accessible for as KeyedService from IKeyedServiceProvider</param>
    /// <param name="feature">Feature flag</param>
    public ServiceAttribute(Type? serviceType = null, Lifetime lifetime = Lifetime.Transient, string? key = null, TFeatureFlag feature = default) : base(feature)
    {
        ServiceType = serviceType;
        Lifetime = lifetime;
        Key = key;
    }

    public Lifetime Lifetime { get; init; }

    public string? Key { get; init; }
}