using System.Reflection;
using System.Runtime.InteropServices;
using IL.AttributeBasedDI.Attributes;
using IL.AttributeBasedDI.Helpers;
using IL.AttributeBasedDI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IL.AttributeBasedDI.Extensions;

internal static class ServiceWithOptionsAttributeRegistration
{
#if NET7_0_OR_GREATER
    public static void RegisterClassesWithServiceAttributesWithOptions(this IServiceCollection serviceCollection, IConfiguration? configuration = null,
        params Type[] types)
    {
        var serviceRegistrations = types
            .Where(type => type.GetCustomAttributes(typeof(ServiceWithOptionsAttribute<>)).Any())
            .SelectMany(type =>
            {
                return type
                    .GetCustomAttributes(typeof(ServiceWithOptionsAttribute<>))
                    .Select(attribute =>
                    {
                        RegisterOptionsFromAttribute(serviceCollection, configuration, attribute);
                        var @base = attribute as ServiceAttribute;
                        return @base!.ToRegistrationEntry(type);
                    });
            })
            .ToList();

        foreach (var serviceRegistrationEntry in CollectionsMarshal.AsSpan(serviceRegistrations))
        {
            if (serviceRegistrationEntry.ServiceType == null)
            {
                serviceCollection.AddServiceWithLifetime(serviceRegistrationEntry.ImplementationType,
                    serviceRegistrationEntry.ServiceLifetime,
#if NET8_0_OR_GREATER
                    serviceRegistrationEntry.Key);
#else
                    string.Empty);
#endif
            }
            else
            {
                serviceCollection.AddServiceWithLifetime(serviceRegistrationEntry.ServiceType,
                    serviceRegistrationEntry.ImplementationType,
                    serviceRegistrationEntry.ServiceLifetime,
#if NET8_0_OR_GREATER
                    serviceRegistrationEntry.Key);
#else
                    string.Empty);
#endif
            }
        }
    }

    private static readonly MethodInfo ConfigureMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
        .GetMethods()
        .Where(x => x.Name == nameof(OptionsConfigurationServiceCollectionExtensions.Configure))
        .First(x =>
            {
                var parameters = x.GetParameters();
                return parameters.Length == 2
                       && parameters[0].ParameterType == typeof(IServiceCollection)
                       && parameters[1].ParameterType == typeof(IConfiguration);
            }
        );

    private static void RegisterOptionsFromAttribute(IServiceCollection serviceCollection, IConfiguration? configuration, Attribute attribute)
    {
        var configurationPathBase = attribute as IAttributeWithOptionsConfigurationPath;
        var configurationPath = configurationPathBase!.ConfigurationPath ?? string.Empty;
        var genericTypeUsedOnAttributeDeclaration = attribute.GetType().GetGenericArguments().First();

        if (!string.IsNullOrEmpty(configurationPath) && configuration != null)
        {
            ConfigureMethod
                .MakeGenericMethod(genericTypeUsedOnAttributeDeclaration)
                .Invoke(null, new object[] { serviceCollection, configuration.GetSection(configurationPath) });
        }
    }
#endif
}