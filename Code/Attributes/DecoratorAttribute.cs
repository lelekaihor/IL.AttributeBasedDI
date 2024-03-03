namespace IL.AttributeBasedDI.Attributes;

/// <summary>
/// Attribute for reflection based class detection and registration in Microsoft DI container as Decorator for existing service.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DecoratorAttribute : DependencyInjectionAttributeBase
{
    /// <summary>
    /// Decorator Attribute constructor
    /// </summary>
    /// <param name="serviceType">Specifies which service is target for decoration.
    /// If left null/default service will be automatically resolved to first interface current class implements.</param>
    /// <param name="decorationLayer">Defines order of decoration. Lower decoration order will be closer to original implementation in chain of execution order.
    /// And, respectively, decorator with highest DecorationOrder will be executed last.</param>
    public DecoratorAttribute(Type? serviceType = default, int decorationLayer = 1)
    {
        ServiceType = serviceType;
        DecorationOrder = decorationLayer < 1 ? 1 : decorationLayer;
    }

    /// <summary>
    /// Defines order of decoration. Lower decoration order will be closer to original implementation in chain of execution order.
    /// And, respectively, decorator with highest DecorationOrder will be executed last.
    /// </summary>
    public int DecorationOrder { get; init; }
}