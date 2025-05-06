using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace IL.AttributeBasedDI.Tests.DI;

using Attributes;
using Extensions;
using Options;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class ServiceTestOptions : IServiceConfiguration
{
    static string IServiceConfiguration.ConfigurationPath => "AppSettings:Test";

    public string Option1 { get; set; } = "test123";
}

[ServiceWithOptions<ServiceTestOptions>(Lifetime = ServiceLifetime.Singleton)]
public class TestServiceWithOptions
{
    private readonly ServiceTestOptions _serviceConfiguration;

    public TestServiceWithOptions(IOptions<ServiceTestOptions> options)
    {
        _serviceConfiguration = options.Value;
    }

    public string GetOption1Value() => _serviceConfiguration.Option1;
}

public class ServiceTestOptions1 : IServiceConfiguration
{
    static string? IServiceConfiguration.ConfigurationPath => null;

    public string Option1 { get; set; } = "test123";
}

[ServiceWithOptions<ServiceTestOptions1>(Lifetime = ServiceLifetime.Singleton)]
public class TestServiceWithOptions1
{
    private readonly ServiceTestOptions1 _serviceConfiguration;

    public TestServiceWithOptions1(IOptions<ServiceTestOptions1> options)
    {
        _serviceConfiguration = options.Value;
    }

    public string GetOption1Value() => _serviceConfiguration.Option1;
}

public class ServiceAttributesWithOptionsTests
{
    [Fact]
    public void ServiceWithOptionsSuccessfullyRegistered()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act
        var appSettings = """
                          {
                              "AppSettings": {
                                  "Test" : {
                                      "Option1":"test12345"
                                  }
                              }
                          }
                          """;

        var builder = new ConfigurationBuilder();
        builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings)));
        var configuration = builder.Build();
        serviceCollection.AddServiceAttributeBasedDependencyInjection(configuration);
        var sp = serviceCollection.BuildServiceProvider();

        // Assert
        var service1 = sp.GetRequiredService<TestServiceWithOptions>();
        Assert.NotNull(service1);
        Assert.Equal("test12345", service1.GetOption1Value());
        var service2 = sp.GetRequiredService<TestServiceWithOptions1>();
        Assert.NotNull(service2);
        Assert.Equal("test123", service2.GetOption1Value());
    }
}