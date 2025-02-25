namespace IL.AttributeBasedDI.Exceptions;

public class ServiceDecorationException : InvalidOperationException
{
    public ServiceDecorationException(string message) : base(message)
    {
    }
}