namespace IL.AttributeBasedDI.Options;
#if NET7_0_OR_GREATER
public interface IServiceConfiguration
{
   public static abstract string? ConfigurationPath { get; }
}
#endif