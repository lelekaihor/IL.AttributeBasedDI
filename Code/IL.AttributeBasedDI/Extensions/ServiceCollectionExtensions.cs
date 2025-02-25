using IL.AttributeBasedDI.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IL.AttributeBasedDI.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServiceAttributeBasedDependencyInjection(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddServiceAttributeBasedDependencyInjection("*");
    }

    public static void AddServiceAttributeBasedDependencyInjection(this IServiceCollection serviceCollection, params string[] assemblyFilters)
    {
        foreach (var solutionItemWildcard in assemblyFilters.AsSpan())
        {
            serviceCollection.RegisterClassesWithServiceAttributeAndDecorators(null, solutionItemWildcard);
        }
    }

    public static void AddServiceAttributeBasedDependencyInjectionWithOptions(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddServiceAttributeBasedDependencyInjectionWithOptions(configuration, "*");
    }

    public static void AddServiceAttributeBasedDependencyInjectionWithOptions(this IServiceCollection serviceCollection, IConfiguration configuration,
        params string[] assemblyFilters)
    {
        foreach (var solutionItemWildcard in assemblyFilters.AsSpan())
        {
            serviceCollection.RegisterClassesWithServiceAttributeAndDecorators(configuration, solutionItemWildcard);
        }
    }
}