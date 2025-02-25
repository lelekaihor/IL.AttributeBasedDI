using System.Reflection;
using System.Runtime.InteropServices;
using IL.AttributeBasedDI.Attributes;
using IL.AttributeBasedDI.Exceptions;
using IL.AttributeBasedDI.Helpers;
using IL.AttributeBasedDI.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IL.AttributeBasedDI.Extensions;

internal static class DecoratorAttributeRegistration
{
    public static void RegisterClassesWithDecoratorAttributes<TFeatureFlag>(this IServiceCollection serviceCollection,
        TFeatureFlag activeFeatures,
        params Type[] types)
        where TFeatureFlag : struct, Enum
    {
        var serviceDecorations = types
            .Where(type => type.GetCustomAttribute<DecoratorAttribute<TFeatureFlag>>() != null)
            .Select(type =>
            {
                var decoratorAttribute = type.GetCustomAttribute<DecoratorAttribute<TFeatureFlag>>();
                return new
                {
                    decoratorAttribute!.Key,
                    decoratorAttribute.DecorationOrder,
                    decoratorAttribute.Feature,
                    ServiceType = ServiceRegistrationHelper.GetServiceTypeBasedOnDependencyInjectionAttribute(type, decoratorAttribute),
                    DecoratorImplementationType = type
                };
            })
            .Where(x => FeatureFlagHelper.IsFeatureEnabled(activeFeatures, x.Feature))
            .OrderBy(x => x.DecorationOrder)
            .ToList();

        foreach (var serviceDecorationEntry in CollectionsMarshal.AsSpan(serviceDecorations))
        {
            if (serviceDecorationEntry.ServiceType == null)
            {
                throw new ServiceDecorationException($"Can't determine service to decorate. Decorator type: {serviceDecorationEntry.DecoratorImplementationType.FullName}");
            }

            serviceCollection.AddDecoratorForService(serviceDecorationEntry.ServiceType,
                serviceDecorationEntry.DecoratorImplementationType,
                serviceDecorationEntry.Key);
        }
    }

    //Credits to https://greatrexpectations.com/2018/10/25/decorators-in-net-core-with-dependency-injection
    private static void AddDecoratorForService(this IServiceCollection serviceCollection, Type serviceType, Type decoratorImplementationType, string? key)
    {
        var objectFactory = ActivatorUtilities.CreateFactory(decoratorImplementationType, [serviceType]);
        var descriptorsToDecorate = serviceCollection
            .Where(s =>
            {
                var valid = s.ServiceType == serviceType;
                if (!string.IsNullOrEmpty(key))
                {
                    valid = s.ServiceKey?.ToString() == key;
                }

                return valid;
            })
            .ToList();

        if (descriptorsToDecorate.Count == 0)
        {
            throw new ServiceDecorationException($"No services registered for type {serviceType} in ServiceCollection, Decoration is impossible.");
        }

        foreach (var descriptor in CollectionsMarshal.AsSpan(descriptorsToDecorate))
        {
            serviceCollection.Replace(!string.IsNullOrEmpty(key)
                ? ServiceDescriptor.DescribeKeyed(
                    serviceType,
                    key,
                    (serviceProvider, _) => objectFactory(serviceProvider, [serviceProvider.CreateInstance(descriptor)]),
                    descriptor.Lifetime
                )
                : ServiceDescriptor.Describe(
                    serviceType,
                    implementationFactory => objectFactory(implementationFactory, [implementationFactory.CreateInstance(descriptor)]),
                    descriptor.Lifetime)
            );
        }
    }

    private static object CreateInstance(this IServiceProvider serviceProvider, ServiceDescriptor serviceDescriptor) =>
        serviceDescriptor switch
        {
            { IsKeyedService: true, KeyedImplementationInstance: not null } => serviceDescriptor.KeyedImplementationInstance,
            { IsKeyedService: true, KeyedImplementationFactory: not null } => serviceDescriptor.KeyedImplementationFactory(serviceProvider, serviceDescriptor.ServiceKey),
            { IsKeyedService: true, KeyedImplementationInstance: null, KeyedImplementationFactory: null } => ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider,
                serviceDescriptor.KeyedImplementationType!),
            { IsKeyedService: false, ImplementationInstance: not null } => serviceDescriptor.ImplementationInstance,
            { IsKeyedService: false, ImplementationFactory: not null } => serviceDescriptor.ImplementationFactory(serviceProvider),
            _ => ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, serviceDescriptor.ImplementationType!)
        };
}