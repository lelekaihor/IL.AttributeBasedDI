using IL.AttributeBasedDI.Attributes;
using IL.AttributeBasedDI.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IL.AttributeBasedDI.Tests.DI;

public class TestClassForGenericConstraint;

public interface IGenericInterface<T>;

[Service]
public class GenericInterfaceImplementation : IGenericInterface<TestClassForGenericConstraint[]>;

[Decorator(typeof(IGenericInterface<>), TreatOpenGenericsAsWildcard = true)]
public class GenericInterfaceDecorator<T>(IGenericInterface<T> originalService) : IGenericInterface<T>
{
    public Type DecoratedService() => originalService.GetType();
}

public class ServiceRegistrationWithConstraintsAndTreatOpenGenericsAsWildcard
{
    [Fact]
    public void GenericInterfaceDecorator_ShouldDecorate_InterfacesWithGenericConstraints()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        var builder = new ConfigurationBuilder();
        var configuration = builder.Build();
        serviceCollection.AddServiceAttributeBasedDependencyInjection(configuration);
        var sp = serviceCollection.BuildServiceProvider();

        // Act
        var decoratedService = sp.GetService<IGenericInterface<TestClassForGenericConstraint[]>>();

        // Assert
        Assert.NotNull(decoratedService);
        Assert.IsType<GenericInterfaceDecorator<TestClassForGenericConstraint[]>>(decoratedService);

        var decorator = (GenericInterfaceDecorator<TestClassForGenericConstraint[]>)decoratedService;
        Assert.Equal(typeof(GenericInterfaceImplementation), decorator.DecoratedService());
    }
}