using System.Reflection;
using System.Runtime.InteropServices;
using IL.AttributeBasedDI.Attributes;
using IL.AttributeBasedDI.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
            serviceCollection.RegisterClassesWithServiceAttributeAndDecorators(solutionItemWildcard);
        }
    }

    private static void RegisterClassesWithServiceAttributeAndDecorators(this IServiceCollection serviceCollection, params string[] assemblyFilters)
    {
        var assemblies = TypesAndAssembliesHelper.GetAssemblies(assemblyFilters);
        var allTypes = GetAllTypesFromAssemblies(assemblies);
        serviceCollection.RegisterClassesWithServiceAttributeInAssemblies(allTypes);
        serviceCollection.RegisterClassesWithDecoratorAttributeInAssemblies(allTypes);
    }

    #region ServiceAttribute

    private static void RegisterClassesWithServiceAttributeInAssemblies(this IServiceCollection serviceCollection, params Type[] types)
    {
        var serviceRegistrations = types
            .Where(type => type.GetCustomAttribute<ServiceAttribute>() != default)
            .Select(type => new
            {
                ServiceLifetime = type.GetCustomAttribute<ServiceAttribute>()!.Lifetime,
                ServiceType = GetServiceTypeBasedOnDependencyInjectionAttribute<ServiceAttribute>(type),
                ImplementationType = type
            })
            .ToList();

        foreach (var serviceRegistrationEntry in CollectionsMarshal.AsSpan(serviceRegistrations))
        {
            if (serviceRegistrationEntry.ServiceType == null)
            {
                serviceCollection.AddServiceWithLifetime(serviceRegistrationEntry.ImplementationType, serviceRegistrationEntry.ServiceLifetime);
            }
            else
            {
                serviceCollection.AddServiceWithLifetime(serviceRegistrationEntry.ServiceType, serviceRegistrationEntry.ImplementationType,
                    serviceRegistrationEntry.ServiceLifetime);
            }
        }
    }

    private static void AddServiceWithLifetime(this IServiceCollection serviceCollection, Type type, Lifetime lifetime)
    {
        switch (lifetime)
        {
            case Lifetime.Singleton:
                serviceCollection.AddSingleton(type);
                break;

            case Lifetime.Transient:
                serviceCollection.AddTransient(type);
                break;

            case Lifetime.Scoped:
                serviceCollection.AddScoped(type);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
    }

    private static void AddServiceWithLifetime(this IServiceCollection serviceCollection, Type serviceType, Type implementationType, Lifetime lifetime)
    {
        switch (lifetime)
        {
            case Lifetime.Singleton:
                serviceCollection.AddSingleton(serviceType, implementationType);
                break;

            case Lifetime.Transient:
                serviceCollection.AddTransient(serviceType, implementationType);
                break;

            case Lifetime.Scoped:
                serviceCollection.AddScoped(serviceType, implementationType);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
    }

    #endregion ServiceAttribute

    #region DecoratorAttribute

    private static void RegisterClassesWithDecoratorAttributeInAssemblies(this IServiceCollection serviceCollection, params Type[] types)
    {
        var serviceDecorations = types
            .Where(type => type.GetCustomAttribute<DecoratorAttribute>() != default)
            .Select(type => new
            {
                type.GetCustomAttribute<DecoratorAttribute>()!.DecorationOrder,
                ServiceType = GetServiceTypeBasedOnDependencyInjectionAttribute<DecoratorAttribute>(type),
                DecoratorImplementationType = type
            })
            .OrderBy(x => x.DecorationOrder)
            .ToList();

        foreach (var serviceDecorationEntry in CollectionsMarshal.AsSpan(serviceDecorations))
        {
            if (serviceDecorationEntry.ServiceType == null)
            {
                throw new InvalidOperationException($"Can't determine service to decorate. Decorator type: {serviceDecorationEntry.DecoratorImplementationType.FullName}");
            }

            serviceCollection.AddDecoratorForService(serviceDecorationEntry.ServiceType, serviceDecorationEntry.DecoratorImplementationType);
        }
    }

    //Credits to https://greatrexpectations.com/2018/10/25/decorators-in-net-core-with-dependency-injection
    private static void AddDecoratorForService(this IServiceCollection serviceCollection, Type serviceType, Type decoratorImplementationType)
    {
        var objectFactory = ActivatorUtilities.CreateFactory(
            decoratorImplementationType,
            new[] { serviceType });

        var descriptorsToDecorate = serviceCollection
            .Where(s => s.ServiceType == serviceType)
            .ToList();

        if (descriptorsToDecorate.Count == 0)
        {
            throw new InvalidOperationException($"No services registered for type {serviceType} in ServiceCollection, Decoration is impossible.");
        }

        foreach (var descriptor in CollectionsMarshal.AsSpan(descriptorsToDecorate))
        {
            serviceCollection.Replace(ServiceDescriptor.Describe(
                serviceType,
                implementationFactory => objectFactory(implementationFactory, new[] { implementationFactory.CreateInstance(descriptor) }),
                descriptor.Lifetime)
            );
        }
    }

    private static object CreateInstance(this IServiceProvider serviceProvider, ServiceDescriptor serviceDescriptor)
    {
        if (serviceDescriptor.ImplementationInstance != null)
        {
            return serviceDescriptor.ImplementationInstance;
        }

        if (serviceDescriptor.ImplementationFactory != null)
        {
            return serviceDescriptor.ImplementationFactory(serviceProvider);
        }

        return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, serviceDescriptor.ImplementationType!);
    }

    #endregion DecoratorAttribute


    private static Type? GetServiceTypeBasedOnDependencyInjectionAttribute<T>(Type sourceType) where T : DependencyInjectionAttributeBase
    {
        var dependencyInjectionAttributeBase = sourceType.GetCustomAttribute<T>();
        if (dependencyInjectionAttributeBase == null)
        {
            return default;
        }

        return dependencyInjectionAttributeBase.FindServiceTypeAutomatically ? ExtractServiceTypeFromInterfaces(sourceType) : dependencyInjectionAttributeBase.ServiceType;
    }

    private static Type? ExtractServiceTypeFromInterfaces(Type sourceType)
    {
        return sourceType.GetInterfaces().FirstOrDefault();
    }

    private static Type[] GetAllTypesFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .Where(assembly => !assembly.IsDynamic)
            .SelectMany(TypesAndAssembliesHelper.GetExportedTypes)
            .Where(type => type is { IsAbstract: false, IsGenericTypeDefinition: false })
            .ToArray();
    }
}