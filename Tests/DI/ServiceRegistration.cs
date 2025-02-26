using IL.AttributeBasedDI.Attributes;
using IL.AttributeBasedDI.Extensions;
using IL.AttributeBasedDI.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IL.AttributeBasedDI.Tests.DI;

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
}