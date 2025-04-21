namespace IL.AttributeBasedDI.Options;

public interface IServiceConfiguration
{
    public static abstract string? ConfigurationPath { get; }
}