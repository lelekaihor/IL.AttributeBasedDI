using IL.AttributeBasedDI.Attributes;
using IL.AttributeBasedDI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IL.AttributeBasedDI.Tests.DI;

[Service(Key = "test")]
[Service(Key = "secondaryKeyForTest")]
//#############################################
public sealed class TestClass;

//#############################################
public interface ITestInterface;

[Service(Key = "test1")]
public sealed class TestClass1 : ITestInterface;

//#############################################
public interface ITestInterface1
{
    List<string> IdentifySelf();
}

[Service(Key = "test2")]
public sealed class TestClass2 : ITestInterface1
{
    public List<string> IdentifySelf()
    {
        return [nameof(TestClass2)];
    }
}

[Decorator(Key = "test2", DecorationOrder = 1)]
public sealed class TestClass2Decorator1(ITestInterface1 testInterface1) : ITestInterface1
{
    public List<string> IdentifySelf()
    {
        var result = testInterface1.IdentifySelf();
        result.Add(nameof(TestClass2Decorator1));
        return result;
    }
}

[Decorator(Key = "test2", DecorationOrder = 2)]
public sealed class TestClass2Decorator2(ITestInterface1 testInterface1) : ITestInterface1
{
    public List<string> IdentifySelf()
    {
        var result = testInterface1.IdentifySelf();
        result.Add(nameof(TestClass2Decorator2));
        return result;
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
public interface IWidelyUsedInterface;

[Service]
public sealed class TestWidelyUsedService1 : IWidelyUsedInterface;

[Service]
public sealed class TestWidelyUsedService2 : IWidelyUsedInterface;

[Decorator]
public sealed class TestCommonDecoratorForWidelyUsedServices(IWidelyUsedInterface widelyUsedService) : IWidelyUsedInterface
{
    public string NameYourDependency()
    {
        return widelyUsedService.GetType().Name;
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
        Assert.NotNull(sp.GetRequiredKeyedService<TestClass>("secondaryKeyForTest"));
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
    public void KeyedServices_Registered_By_Key_And_Resolved_By_Interface_ConsidersDecorators_In_Correct_Order()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddServiceAttributeBasedDependencyInjection();
        var sp = serviceCollection.BuildServiceProvider();
        var resolvedService = sp.GetRequiredKeyedService<ITestInterface1>("test2");
        var selfIdentificationResult = resolvedService.IdentifySelf();

        // Assert
        Assert.Equal([nameof(TestClass2), nameof(TestClass2Decorator1), nameof(TestClass2Decorator2)], selfIdentificationResult);
        Assert.NotNull(resolvedService);
        Assert.True(resolvedService is TestClass2Decorator2);
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

    [Fact]
    public void Decorator_Can_Be_Applied_To_Multiple_Services()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        serviceCollection.AddServiceAttributeBasedDependencyInjection();
        var sp = serviceCollection.BuildServiceProvider();
        var widelyUsedTestServices = sp.GetServices<IWidelyUsedInterface>().ToList();
        var originalServices = widelyUsedTestServices
            .Select(x => x as TestCommonDecoratorForWidelyUsedServices)
            .Select(x => x?.NameYourDependency())
            .ToList();

        // Assert
        Assert.True(widelyUsedTestServices.All(x => x is TestCommonDecoratorForWidelyUsedServices));
        Assert.Contains(nameof(TestWidelyUsedService1), originalServices);
        Assert.Contains(nameof(TestWidelyUsedService2), originalServices);
    }
}