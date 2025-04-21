using IL.AttributeBasedDI.Helpers;
using IL.AttributeBasedDI.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IL.AttributeBasedDI.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServiceAttributeBasedDependencyInjection(this IServiceCollection serviceCollection,
        IConfiguration configuration,
        Action<FeatureBasedDIOptions>? configureOptions = null,
        string[]? assemblyFilters = null)
    {
        PreventEmptyAssemblyFilters(ref assemblyFilters);

        var options = new FeatureBasedDIOptions(configuration);
        configureOptions?.Invoke(options);

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

    private static void PreventEmptyAssemblyFilters(ref string[]? assemblyFilters)
    {
        if (assemblyFilters == null || assemblyFilters.Length == 0)
        {
            assemblyFilters = ["*"];
        }
    }
}