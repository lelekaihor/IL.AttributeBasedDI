using System.Text;
using IL.AttributeBasedDI.Attributes;
using Microsoft.Extensions.Configuration;

namespace IL.AttributeBasedDI.Tests.DI;

using Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[Flags]
public enum Features
{
    None = 0,
    FeatureA = 1 << 0,
    FeatureB = 1 << 1,
    FeatureC = 1 << 2
}

[Service<Features>(Feature = Features.FeatureA)]
public class Test1
{
}

[Service<Features>(Feature = Features.FeatureB)]
public class Test2
{
}

[Service<Features>(Feature = Features.FeatureC)]
public class Test3
{
}

public class FeatureEnabledAttributesTests
{
    [Fact]
    public void OnlyServiceWithActivatedFeaturesSuccessfullyRegistered_AppSettings()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var appSettings = """
                          {
                              "DIFeatureFlags": ["FeatureA", "FeatureC"]
                          }
                          """;

        var builder = new ConfigurationBuilder();
        builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings)));
        var configuration = builder.Build();
        serviceCollection.AddServiceAttributeBasedDependencyInjectionWithOptions<Features>(configuration);
        var sp = serviceCollection.BuildServiceProvider();

        // Assert
        var service1 = sp.GetRequiredService<Test1>();
        Assert.NotNull(service1);
        var service2 = sp.GetService<Test2>();
        Assert.Null(service2);
        var service3 = sp.GetService<Test3>();
        Assert.NotNull(service3);
    }

    [Fact]
    public void OnlyServiceWithActivatedFeaturesSuccessfullyRegistered_OptionsAction()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();


        var builder = new ConfigurationBuilder();
        var configuration = builder.Build();
        serviceCollection.AddServiceAttributeBasedDependencyInjectionWithOptions<Features>(configuration,
            options => options.ActiveFeatures = Features.FeatureA | Features.FeatureC);
        var sp = serviceCollection.BuildServiceProvider();

        // Assert
        var service1 = sp.GetRequiredService<Test1>();
        Assert.NotNull(service1);
        var service2 = sp.GetService<Test2>();
        Assert.Null(service2);
        var service3 = sp.GetService<Test3>();
        Assert.NotNull(service3);
    }
}