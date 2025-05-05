using IL.AttributeBasedDI.FeatureFlags;
using IL.AttributeBasedDI.Helpers;
using IL.AttributeBasedDI.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IL.AttributeBasedDI.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServiceAttributeBasedDependencyInjection(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        params string[] assemblyFilters)
    {
        serviceCollection.AddServiceAttributeBasedDependencyInjection(configuration, null, assemblyFilters);
    }

    public static void AddServiceAttributeBasedDependencyInjection(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Action<FeatureBasedDIOptions>? configureOptions = null,
        params string[] assemblyFilters)
    {
        PreventEmptyAssemblyFilters(ref assemblyFilters);

        var options = new FeatureBasedDIOptions(configuration);
        configureOptions?.Invoke(options);
        serviceCollection.ConfigureOptions(options.ActiveFeatures);

        var methodInfo = typeof(ServiceRegistrationHelper).GetMethod(nameof(ServiceRegistrationHelper.RegisterClassesWithServiceAttributeAndDecorators));

        foreach (var filter in assemblyFilters!)
        {
            foreach (var featureEnum in options.ActiveFeatures.AllFeatures)
            {
                var enumType = featureEnum.GetType();

                var genericMethod = methodInfo!.MakeGenericMethod(enumType);

                genericMethod.Invoke(null,
                    [
                        serviceCollection,
                        featureEnum,
                        configuration,
                        options.ThrowWhenDecorationTypeNotFound,
                        new[] { filter }
                    ]
                );
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