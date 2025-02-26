using IL.AttributeBasedDI.Models;

namespace IL.AttributeBasedDI.Attributes;

public sealed class DecoratorAttribute : DecoratorAttribute<FeaturesNoop>
{
    /// <summary>
    /// Decorator Attribute constructor
    /// </summary>
    /// <param name="serviceType">Specifies which service is target for decoration.
    ///     If left null/default service will be automatically resolved to first interface current class implements.</param>
    /// <param name="decorationOrder">Defines order of decoration. Lower decoration order will be closer to original implementation in chain of execution order.
    ///     And, respectively, decorator with highest DecorationOrder will be executed last.</param>
    /// <param name="key">Specifies key which current service will be accessible for as KeyedService from IKeyedServiceProvider</param>
    /// <param name="feature">Feature flag</param>
    public DecoratorAttribute(Type? serviceType = null, int decorationOrder = 1, string? key = null) : base(serviceType, decorationOrder, key)
    {
    }
}

/// <summary>
/// Attribute for reflection based class detection and registration in Microsoft DI container as Decorator for existing service.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DecoratorAttribute<TFeatureFlag> : DependencyInjectionAttributeBase<TFeatureFlag> where TFeatureFlag : struct, Enum

{
    /// <summary>
    /// Decorator Attribute constructor
    /// </summary>
    /// <param name="serviceType">Specifies which service is target for decoration.
    ///     If left null/default service will be automatically resolved to first interface current class implements.</param>
    /// <param name="decorationOrder">Defines order of decoration. Lower decoration order will be closer to original implementation in chain of execution order.
    ///     And, respectively, decorator with highest DecorationOrder will be executed last.</param>
    /// <param name="key">Specifies key which current service will be accessible for as KeyedService from IKeyedServiceProvider</param>
    /// <param name="feature">Feature flag</param>
    public DecoratorAttribute(Type? serviceType = null, int decorationOrder = 1, string? key = null, TFeatureFlag feature = default) : base(feature)
    {
        ServiceType = serviceType;
        DecorationOrder = decorationOrder < 1 ? 1 : decorationOrder;
        Key = key;
    }

    /// <summary>
    /// Defines order of decoration. Lower decoration order will be closer to original implementation in chain of execution order.
    /// And, respectively, decorator with highest DecorationOrder will be executed last.
    /// </summary>
    public int DecorationOrder { get; init; }

    public string? Key { get; init; }
}