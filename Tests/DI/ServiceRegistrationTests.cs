using IL.AttributeBasedDI.Attributes;
using IL.AttributeBasedDI.Extensions;
using IL.AttributeBasedDI.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IL.AttributeBasedDI.Tests.DI;

public interface ICommonInterface;

[Service(Key = "test-original1")]
public class OriginalService1 : ICommonInterface;

[Service(Key = "test-original2")]
public class OriginalService2 : ICommonInterface;

[Decorator(Key = "test-*")]
public class DecoratorOfTestServices(ICommonInterface originalService1Or2) : ICommonInterface
{
    public Type DecoratedService() => originalService1Or2.GetType();
}

[Service(typeof(Test123))]
[ServiceWithOptions<OptionsProvier>(typeof(Test123))]
public class Test123
{
}

public class OptionsProvier : IServiceConfiguration
{
    public static string? ConfigurationPath { get; } = string.Empty;
}

public class ServiceRegistration
{
    [Fact]
    public void DefaultServiceRegistration()
    {
        var serviceCollection = new ServiceCollection();

        var builder = new ConfigurationBuilder();
        var configuration = builder.Build();
        serviceCollection.AddServiceAttributeBasedDependencyInjectionWithOptions(configuration);
        var sp = serviceCollection.BuildServiceProvider();

        Assert.NotNull(sp.GetService<Test123>());
    }

    [Fact]
    public void DecoratorOfTestServices_ShouldDecorate_OriginalService1()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        var builder = new ConfigurationBuilder();
        var configuration = builder.Build();
        serviceCollection.AddServiceAttributeBasedDependencyInjectionWithOptions(configuration);
        var sp = serviceCollection.BuildServiceProvider();

        // Act
        var decoratedService1 = sp.GetKeyedService<ICommonInterface>("test-original1");
        var decoratedService2 = sp.GetKeyedService<ICommonInterface>("test-original2");

        // Assert
        Assert.NotNull(decoratedService1);
        Assert.IsType<DecoratorOfTestServices>(decoratedService1);
        Assert.NotNull(decoratedService2);
        Assert.IsType<DecoratorOfTestServices>(decoratedService2);

        var decorator1 = (DecoratorOfTestServices)decoratedService1;
        Assert.Equal(typeof(OriginalService1), decorator1.DecoratedService());

        var decorator2 = (DecoratorOfTestServices)decoratedService2;
        Assert.Equal(typeof(OriginalService2), decorator2.DecoratedService());
    }
}