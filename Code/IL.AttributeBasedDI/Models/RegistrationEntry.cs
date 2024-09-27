using IL.AttributeBasedDI.Attributes;

namespace IL.AttributeBasedDI.Models;

internal sealed class RegistrationEntry
{
    public Lifetime ServiceLifetime { get; set; }

    public Type? ServiceType { get; set; }

    public Type ImplementationType { get; set; } = null!;
#if NET8_0_OR_GREATER
    public string? Key { get; set; }
#endif
}