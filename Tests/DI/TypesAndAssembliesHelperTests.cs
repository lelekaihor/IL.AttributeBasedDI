using IL.AttributeBasedDI.Attributes;
using IL.AttributeBasedDI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IL.AttributeBasedDI.Tests.DI;
#if NET8_0_OR_GREATER

[Service(Key = "test")]
//#############################################
public sealed class TestClass;

//#############################################
public interface ITestInterface;

[Service(Key = "test1")]
public sealed class TestClass1 : ITestInterface;

//#############################################
public interface ITestInterface1;

[Service(Key = "test2")]
public sealed class TestClass2 : ITestInterface1;

[Decorator(Key = "test2")]
public sealed class TestClass2Decorator : ITestInterface1
{
    private readonly ITestInterface1 _testInterface1;

    public TestClass2Decorator(ITestInterface1 testInterface1)
    {
        _testInterface1 = testInterface1;
    }
}

//#############################################
public interface IGenericTestInterface<T> where T : class, new();

[Service(Key = "generic")]
public sealed class TestGenericClass : IGenericTestInterface<object>;

//#############################################
public interface IGenericConstaint;

public class GenericConstaintImplementation : IGenericConstaint;

public interface IGenericTestInterface1<out T> where T : IGenericConstaint
{
    public T Test();
}

[Service(Key = "generic1")]
public sealed class TestGenericClass1 : IGenericTestInterface1<IGenericConstaint>
{
    public IGenericConstaint Test()
    {
        return new GenericConstaintImplementation();
    }
}

//#############################################
public class DiTests
{
    [Fact]
    public void KeyedServices_Registered_By_Key()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddServiceAttributeBasedDependencyInjection();
        var sp = serviceCollection.BuildServiceProvider();

        // Assert
        Assert.NotNull(sp.GetRequiredKeyedService<TestClass>("test"));
    }
    
    [Fact]
    public void KeyedServices_Registered_By_Key_And_Resolved_By_Interface()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddServiceAttributeBasedDependencyInjection();
        var sp = serviceCollection.BuildServiceProvider();

        // Assert
        Assert.NotNull(sp.GetRequiredKeyedService<ITestInterface>("test1"));
        Assert.True(sp.GetRequiredKeyedService<ITestInterface>("test1") is TestClass1);
    }

    [Fact]
    public void KeyedServices_Registered_By_Key_And_Resolved_By_Interface_ConsidersDecorators()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddServiceAttributeBasedDependencyInjection();
        var sp = serviceCollection.BuildServiceProvider();

        // Assert
        Assert.NotNull(sp.GetRequiredKeyedService<ITestInterface1>("test2"));
        Assert.True(sp.GetRequiredKeyedService<ITestInterface1>("test2") is TestClass2Decorator);
    }

    [Fact]
    public void KeyedServices_Registered_By_Key_And_Resolved_By_Generic_Interface()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddServiceAttributeBasedDependencyInjection();
        var sp = serviceCollection.BuildServiceProvider();

        // Assert
        Assert.NotNull(sp.GetRequiredKeyedService<IGenericTestInterface<object>>("generic"));
        Assert.True(sp.GetRequiredKeyedService<IGenericTestInterface<object>>("generic") is TestGenericClass);
    }
    
    [Fact]
    public void KeyedServices_Registered_By_Key_And_Resolved_By_Generic_Interface_With_Generic_Constraint()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddServiceAttributeBasedDependencyInjection();
        var sp = serviceCollection.BuildServiceProvider();

        // Assert
        Assert.NotNull(sp.GetRequiredKeyedService<IGenericTestInterface1<IGenericConstaint>>("generic1"));
        Assert.True(sp.GetRequiredKeyedService<IGenericTestInterface1<IGenericConstaint>>("generic1") is TestGenericClass1);
        Assert.True(sp.GetRequiredKeyedService<IGenericTestInterface1<IGenericConstaint>>("generic1").Test() is GenericConstaintImplementation);
    }
}
#endif