namespace IL.AttributeBasedDI.Attributes;

/// <summary>
/// Attribute for reflection based class detection and registration in Microsoft DI container as Decorator for existing service.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DecoratorAttribute : DependencyInjectionAttributeBase
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// Decorator Attribute constructor
    /// </summary>
    /// <param name="serviceType">Specifies which service is target for decoration.
    /// If left null/default service will be automatically resolved to first interface current class implements.</param>
    /// <param name="decorationOrder">Defines order of decoration. Lower decoration order will be closer to original implementation in chain of execution order.
    /// And, respectively, decorator with highest DecorationOrder will be executed last.</param>
    /// <param name="key">Specifies key which current service will be accessible for as KeyedService from IKeyedServiceProvider</param>
    public DecoratorAttribute(Type? serviceType = default, int decorationOrder = 1, string? key = default)
    {
        ServiceType = serviceType;
        DecorationOrder = decorationOrder < 1 ? 1 : decorationOrder;
        Key = key;
    }
#else
    /// <summary>
    /// Decorator Attribute constructor
    /// </summary>
    /// <param name="serviceType">Specifies which service is target for decoration.
    /// If left null/default service will be automatically resolved to first interface current class implements.</param>
    /// <param name="decorationOrder">Defines order of decoration. Lower decoration order will be closer to original implementation in chain of execution order.
    /// And, respectively, decorator with highest DecorationOrder will be executed last.</param>
    public DecoratorAttribute(Type? serviceType = default, int decorationOrder = 1)
    {
        ServiceType = serviceType;
        DecorationOrder = decorationOrder < 1 ? 1 : decorationOrder;
    }
#endif
    /// <summary>
    /// Defines order of decoration. Lower decoration order will be closer to original implementation in chain of execution order.
    /// And, respectively, decorator with highest DecorationOrder will be executed last.
    /// </summary>
    public int DecorationOrder { get; init; }
#if NET8_0_OR_GREATER
    public string? Key { get; init; }
#endif
}