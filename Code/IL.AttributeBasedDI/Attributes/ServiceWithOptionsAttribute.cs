using IL.AttributeBasedDI.Options;

namespace IL.AttributeBasedDI.Attributes;
#if NET7_0_OR_GREATER
internal interface IAttributeWithOptionsConfigurationPath
{
    public string? ConfigurationPath { get; }
}

public sealed class ServiceWithOptionsAttribute<T> : ServiceAttribute, IAttributeWithOptionsConfigurationPath where T : class, IServiceConfiguration, new()
{
    public string? ConfigurationPath => T.ConfigurationPath;
}
#endif