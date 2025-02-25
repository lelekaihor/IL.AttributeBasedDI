using IL.AttributeBasedDI.Attributes;

namespace IL.AttributeBasedDI.Models;

internal sealed class RegistrationEntry<TFeatureFlag> where TFeatureFlag : struct, Enum
{
    public Lifetime ServiceLifetime { get; init; }

    public Type? ServiceType { get; init; }

    public Type ImplementationType { get; init; } = null!;

    public string? Key { get; init; }

    public TFeatureFlag Feature { get; set; }
}