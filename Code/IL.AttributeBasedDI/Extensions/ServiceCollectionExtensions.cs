using IL.AttributeBasedDI.Helpers;
using IL.AttributeBasedDI.Models;
using IL.AttributeBasedDI.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IL.AttributeBasedDI.Extensions;

public static class ServiceCollectionExtensions
{
    // Non-generic version (no feature flags)
    public static void AddServiceAttributeBasedDependencyInjection(this IServiceCollection serviceCollection,
        params string[] assemblyFilters)
    {
        PreventEmptyAssemblyFilters(ref assemblyFilters);
        serviceCollection.AddServiceAttributeBasedDependencyInjection<FeaturesNoop>(_ => { });
    }

    // Non-generic version with configuration (no feature flags)
    public static void AddServiceAttributeBasedDependencyInjectionWithOptions(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        params string[] assemblyFilters)
    {
        PreventEmptyAssemblyFilters(ref assemblyFilters);
        serviceCollection.AddServiceAttributeBasedDependencyInjectionWithOptions<FeaturesNoop>(configuration, _ => { });
    }

    // Generic version with active features
    public static void AddServiceAttributeBasedDependencyInjection<TFeatureFlag>(
        this IServiceCollection serviceCollection,
        Action<FeatureBasedDIOptions<TFeatureFlag>>? configureOptions,
        params string[] assemblyFilters)
        where TFeatureFlag : struct, Enum
    {
        var options = new FeatureBasedDIOptions<TFeatureFlag>();
        configureOptions?.Invoke(options);

        PreventEmptyAssemblyFilters(ref assemblyFilters);
        foreach (var solutionItemWildcard in assemblyFilters.AsSpan())
        {
            serviceCollection.RegisterClassesWithServiceAttributeAndDecorators(options.ActiveFeatures, null, solutionItemWildcard);
            if (options.ActiveFeatures is not FeaturesNoop)
            {
                serviceCollection.RegisterClassesWithServiceAttributeAndDecorators(FeaturesNoop.None, null, solutionItemWildcard);
            }
        }
    }

    // Generic version with configuration and active features
    public static void AddServiceAttributeBasedDependencyInjectionWithOptions<TFeatureFlag>(
        this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Action<FeatureBasedDIOptions<TFeatureFlag>>? configureOptions = null,
        params string[] assemblyFilters)
        where TFeatureFlag : struct, Enum
    {
        var options = new FeatureBasedDIOptions<TFeatureFlag>();

        // Configure options from appsettings.json
        var featureNames = configuration.GetSection("DIFeatureFlags").Get<string[]>();
        if (featureNames != null)
        {
            options.SetActiveFeaturesFromNames(featureNames);
        }

        // Configure options programmatically
        configureOptions?.Invoke(options);

        PreventEmptyAssemblyFilters(ref assemblyFilters);
        foreach (var solutionItemWildcard in assemblyFilters.AsSpan())
        {
            serviceCollection.RegisterClassesWithServiceAttributeAndDecorators(options.ActiveFeatures, configuration, solutionItemWildcard);
            if (options.ActiveFeatures is not FeaturesNoop)
            {
                serviceCollection.RegisterClassesWithServiceAttributeAndDecorators(FeaturesNoop.None, configuration, solutionItemWildcard);
            }
        }
    }

    private static void PreventEmptyAssemblyFilters(ref string[] assemblyFilters)
    {
        if (assemblyFilters.Length == 0)
        {
            assemblyFilters = ["*"];
        }
    }
}