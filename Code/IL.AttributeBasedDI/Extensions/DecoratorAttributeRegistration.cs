using System.Reflection;
using System.Runtime.InteropServices;
using IL.AttributeBasedDI.Attributes;
using IL.AttributeBasedDI.Exceptions;
using IL.AttributeBasedDI.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IL.AttributeBasedDI.Extensions;

internal static class DecoratorAttributeRegistration
{
    public static void RegisterClassesWithDecoratorAttributes(this IServiceCollection serviceCollection, params Type[] types)
    {
        var serviceDecorations = types
            .Where(type => type.GetCustomAttribute<DecoratorAttribute>() != null)
            .Select(type =>
            {
                var decoratorAttribute = type.GetCustomAttribute<DecoratorAttribute>()!;
                return new
                {
#if NET8_0_OR_GREATER
                    decoratorAttribute.Key,
#endif
                    decoratorAttribute.DecorationOrder,
                    ServiceType = ServiceRegistrationHelper.GetServiceTypeBasedOnDependencyInjectionAttribute(type, decoratorAttribute),
                    DecoratorImplementationType = type
                };
            })
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
#if NET8_0_OR_GREATER
                serviceDecorationEntry.Key);
#else
                string.Empty);
#endif
        }
    }

    //Credits to https://greatrexpectations.com/2018/10/25/decorators-in-net-core-with-dependency-injection
    public static void AddDecoratorForService(this IServiceCollection serviceCollection, Type serviceType, Type decoratorImplementationType, string? key)
    {
        var objectFactory = ActivatorUtilities.CreateFactory(
            decoratorImplementationType,
            new[] { serviceType });

        var descriptorsToDecorate = serviceCollection
            .Where(s =>
            {
                var valid = s.ServiceType == serviceType;
#if NET8_0_OR_GREATER
                if (!string.IsNullOrEmpty(key))
                {
                    valid = s.ServiceKey?.ToString() == key;
                }
#endif
                return valid;
            })
            .ToList();

        if (descriptorsToDecorate.Count == 0)
        {
            throw new ServiceDecorationException($"No services registered for type {serviceType} in ServiceCollection, Decoration is impossible.");
        }

        foreach (var descriptor in CollectionsMarshal.AsSpan(descriptorsToDecorate))
        {
#if NET8_0_OR_GREATER
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
#else
            serviceCollection.Replace(ServiceDescriptor.Describe(
                serviceType,
                implementationFactory => objectFactory(implementationFactory, new[] { implementationFactory.CreateInstance(descriptor) }),
                descriptor.Lifetime)
            );
#endif
        }
    }

    public static object CreateInstance(this IServiceProvider serviceProvider, ServiceDescriptor serviceDescriptor)
    {
#if NET8_0_OR_GREATER
        return serviceDescriptor switch
        {
            { IsKeyedService: true, KeyedImplementationInstance: not null } => serviceDescriptor.KeyedImplementationInstance,
            { IsKeyedService: true, KeyedImplementationFactory: not null } => serviceDescriptor.KeyedImplementationFactory(serviceProvider, serviceDescriptor.ServiceKey),
            { IsKeyedService: true, KeyedImplementationInstance: null, KeyedImplementationFactory: null } => ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider,
                serviceDescriptor.KeyedImplementationType!),
            { IsKeyedService: false, ImplementationInstance: not null } => serviceDescriptor.ImplementationInstance,
            { IsKeyedService: false, ImplementationFactory: not null } => serviceDescriptor.ImplementationFactory(serviceProvider),
            _ => ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, serviceDescriptor.ImplementationType!)
        };

#else
        if (serviceDescriptor.ImplementationInstance != null)
        {
            return serviceDescriptor.ImplementationInstance;
        }

        if (serviceDescriptor.ImplementationFactory != null)
        {
            return serviceDescriptor.ImplementationFactory(serviceProvider);
        }

        return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, serviceDescriptor.ImplementationType!);
#endif
    }
}