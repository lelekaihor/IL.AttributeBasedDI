using System.Reflection;
using System.Runtime.InteropServices;
using IL.AttributeBasedDI.Attributes;
using IL.AttributeBasedDI.Exceptions;
using IL.AttributeBasedDI.Helpers;
using IL.Misc.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IL.AttributeBasedDI.Extensions;

internal static class DecoratorAttributeRegistration
{
    private const string WildcardKey = "*";

    public static void RegisterClassesWithDecoratorAttributes<TFeatureFlag>(this IServiceCollection serviceCollection,
        TFeatureFlag activeFeatures,
        bool throwWhenDecorationTypeNotFound,
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
                    DecoratorImplementationType = type,
                    decoratorAttribute.TreatOpenGenericsAsWildcard
                };
            })
            .Where(x => FeatureFlagHelper.IsFeatureEnabled(activeFeatures, x.Feature))
            .OrderBy(x => x.DecorationOrder)
            .ToList();

        foreach (var serviceDecorationEntry in CollectionsMarshal.AsSpan(serviceDecorations))
        {
            if (serviceDecorationEntry.ServiceType == null)
            {
                if (!throwWhenDecorationTypeNotFound)
                {
                    continue;
                }

                throw new ServiceDecorationException($"Can't determine service to decorate. Decorator type: {serviceDecorationEntry.DecoratorImplementationType.FullName}");
            }

            serviceCollection.AddDecoratorForService(serviceDecorationEntry.ServiceType,
                serviceDecorationEntry.DecoratorImplementationType,
                serviceDecorationEntry.Key,
                serviceDecorationEntry.TreatOpenGenericsAsWildcard,
                throwWhenDecorationTypeNotFound);
        }
    }

    //Credits to https://greatrexpectations.com/2018/10/25/decorators-in-net-core-with-dependency-injection
    private static void AddDecoratorForService(this IServiceCollection serviceCollection,
        Type serviceType,
        Type decoratorImplementationType,
        string? key,
        bool treatOpenGenericsAsWildcard,
        bool throwWhenDecorationTypeNotFound)
    {
        if (!serviceType.IsGenericType || !decoratorImplementationType.IsGenericType)
        {
            HandleNonGenericDecorators(serviceCollection,
                serviceType,
                decoratorImplementationType,
                key,
                throwWhenDecorationTypeNotFound);
        }
        else if (treatOpenGenericsAsWildcard
                 && serviceType.IsGenericType
                 && decoratorImplementationType.ContainsGenericParameters)
        {
            HandleGenericDecoratorsWithTreatOpenGenericsAsWildcard(serviceCollection,
                serviceType,
                decoratorImplementationType,
                key,
                throwWhenDecorationTypeNotFound);
        }
        else
        {
            // standard open generics are not supported for now
        }
    }

    private static void HandleGenericDecoratorsWithTreatOpenGenericsAsWildcard(IServiceCollection serviceCollection,
        Type serviceType,
        Type decoratorImplementationType,
        string? key,
        bool throwWhenDecorationTypeNotFound)
    {
        var descriptorsToDecorate = serviceCollection
            .Where(s =>
            {
                var valid = s.ServiceType.FullName?.StartsWith(serviceType.FullName ?? string.Empty) is true;
                if (valid && !string.IsNullOrEmpty(key))
                {
                    throw new ServiceDecorationException("Wildcard open generics decoration for keyed services is not supported!");
                }

                return valid;
            })
            .ToList();
        if (descriptorsToDecorate.Count == 0)
        {
            if (!throwWhenDecorationTypeNotFound)
            {
                return;
            }

            throw new ServiceDecorationException($"No services registered for type {serviceType.FullName} in ServiceCollection, Decoration is impossible.");
        }

        foreach (var descriptor in CollectionsMarshal.AsSpan(descriptorsToDecorate))
        {
            var genericArguments = descriptor.ServiceType.GetGenericArguments();
            if (genericArguments.Any(x => x.ContainsGenericParameters))
            {
                // standard open generics are not supported for treatOpenGenericsAsWildcard = true
                continue;
            }

            var closedDecoratorType = decoratorImplementationType.MakeGenericType(genericArguments);
            var objectFactory = ActivatorUtilities.CreateFactory(closedDecoratorType, [descriptor.ServiceType]);
            serviceCollection.Replace(
                ServiceDescriptor.Describe(
                    descriptor.ServiceType,
                    implementationFactory => objectFactory(implementationFactory, [implementationFactory.CreateInstance(descriptor)]),
                    descriptor.Lifetime)
            );
        }
    }

    private static void HandleNonGenericDecorators(IServiceCollection serviceCollection,
        Type serviceType,
        Type decoratorImplementationType,
        string? key,
        bool throwWhenDecorationTypeNotFound)
    {
        var descriptorsToDecorate = serviceCollection
            .Where(s =>
            {
                var valid = s.ServiceType == serviceType;
                if (string.IsNullOrEmpty(key))
                {
                    return valid;
                }

                var descriptorServiceKey = s.ServiceKey?.ToString();
                return descriptorServiceKey == key
                       || !string.IsNullOrEmpty(descriptorServiceKey) && IsWildcardKey(key) && descriptorServiceKey.MatchesWildcard(key);
            })
            .ToList();

        if (descriptorsToDecorate.Count == 0)
        {
            if (!throwWhenDecorationTypeNotFound)
            {
                return;
            }

            throw new ServiceDecorationException($"No services registered for type {serviceType.FullName} in ServiceCollection, Decoration is impossible.");
        }

        var objectFactory = ActivatorUtilities.CreateFactory(decoratorImplementationType, [serviceType]);
        foreach (var descriptor in CollectionsMarshal.AsSpan(descriptorsToDecorate))
        {
            serviceCollection.Replace(!string.IsNullOrEmpty(key)
                ? ServiceDescriptor.DescribeKeyed(
                    serviceType,
                    IsWildcardKey(key) ? descriptor.ServiceKey!.ToString() : key,
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

    private static bool IsWildcardKey(string key)
    {
        return key.Contains(WildcardKey);
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