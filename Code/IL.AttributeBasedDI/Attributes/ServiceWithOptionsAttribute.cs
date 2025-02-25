using IL.AttributeBasedDI.Models;
using IL.AttributeBasedDI.Options;

namespace IL.AttributeBasedDI.Attributes;

internal interface IAttributeWithOptionsConfigurationPath
{
    public string? ConfigurationPath { get; }
}

public sealed class ServiceWithOptionsAttribute<T> : ServiceWithOptionsAttribute<T, FeaturesNoop> where T : class, IServiceConfiguration, new()
{
}

public class ServiceWithOptionsAttribute<T, TFeatureFlag> : ServiceAttribute<TFeatureFlag>, IAttributeWithOptionsConfigurationPath
    where T : class, IServiceConfiguration, new()
    where TFeatureFlag : struct, Enum
{
    public string? ConfigurationPath => T.ConfigurationPath;
}