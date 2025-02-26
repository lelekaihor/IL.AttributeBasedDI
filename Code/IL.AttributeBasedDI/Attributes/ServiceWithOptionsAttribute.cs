using IL.AttributeBasedDI.Models;
using IL.AttributeBasedDI.Options;

namespace IL.AttributeBasedDI.Attributes;

internal interface IAttributeWithOptionsConfigurationPath
{
    public string? ConfigurationPath { get; }
}

public sealed class ServiceWithOptionsAttribute<T> : ServiceWithOptionsAttribute<T, FeaturesNoop> where T : class, IServiceConfiguration, new()
{
    /// <summary>
    /// Service Attribute constructor
    /// </summary>
    /// <param name="serviceType">Specifies which service is target for DI registration.
    /// If left null/default service will be automatically retrieved either from first interface current class implements or the class itself will become a serviceType.</param>
    /// <param name="lifetime">Specifies service lifetime.</param>
    /// <param name="key">Specifies key which current service will be accessible for as KeyedService from IKeyedServiceProvider</param>
    public ServiceWithOptionsAttribute(Type? serviceType = null, Lifetime lifetime = Lifetime.Transient, string? key = null) : base(serviceType, lifetime, key)
    {
        ServiceType = serviceType;
        Lifetime = lifetime;
        Key = key;
    }
}

public class ServiceWithOptionsAttribute<T, TFeatureFlag> : ServiceAttribute<TFeatureFlag>, IAttributeWithOptionsConfigurationPath
    where T : class, IServiceConfiguration, new()
    where TFeatureFlag : struct, Enum
{
    /// <summary>
    /// Service Attribute constructor
    /// </summary>
    /// <param name="serviceType">Specifies which service is target for DI registration.
    /// If left null/default service will be automatically retrieved either from first interface current class implements or the class itself will become a serviceType.</param>
    /// <param name="lifetime">Specifies service lifetime.</param>
    /// <param name="key">Specifies key which current service will be accessible for as KeyedService from IKeyedServiceProvider</param>
    public ServiceWithOptionsAttribute(Type? serviceType = null, Lifetime lifetime = Lifetime.Transient, string? key = null) : base(serviceType, lifetime, key)
    {
        ServiceType = serviceType;
        Lifetime = lifetime;
        Key = key;
    }

    public string? ConfigurationPath => T.ConfigurationPath;
}