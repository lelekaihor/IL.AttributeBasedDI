using System.Reflection;
using System.Runtime.InteropServices;
using IL.AttributeBasedDI.Attributes;
using IL.AttributeBasedDI.Helpers;
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
            serviceCollection.AddClassesWithServiceAttribute(solutionItemWildcard);
        }
    }

    private static void AddClassesWithServiceAttribute(this IServiceCollection serviceCollection, params string[] assemblyFilters)
    {
        var assemblies = TypesAndAssembliesHelper.GetAssemblies(assemblyFilters);
        serviceCollection.AddClassesWithServiceAttribute(assemblies);
    }

    private static void AddClassesWithServiceAttribute(this IServiceCollection serviceCollection, params Assembly[] assemblies)
    {
        var typesWithAttributes = assemblies
            .Where(assembly => !assembly.IsDynamic)
            .SelectMany(TypesAndAssembliesHelper.GetExportedTypes)
            .Where(type => !type.IsAbstract && !type.IsGenericTypeDefinition)
            .Select(type => new
            {
                type.GetCustomAttribute<ServiceAttribute>()?.Lifetime,
                ServiceType = GetServiceType(type),
                ImplementationType = type
            })
            .Where(t => t.Lifetime != null)
            .ToList();

        foreach (var type in CollectionsMarshal.AsSpan(typesWithAttributes))
        {
            if (type.ServiceType == null)
            {
                serviceCollection.Add(type.ImplementationType, type.Lifetime!.Value);
            }
            else
            {
                serviceCollection.Add(type.ServiceType, type.ImplementationType, type.Lifetime!.Value);
            }
        }
    }

    private static void Add(this IServiceCollection serviceCollection, Type type, Lifetime lifetime)
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

    private static void Add(this IServiceCollection serviceCollection, Type serviceType, Type implementationType, Lifetime lifetime)
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

    private static Type? GetServiceType(Type sourceType)
    {
        var serviceAttribute = sourceType.GetCustomAttribute<ServiceAttribute>();
        if (serviceAttribute == null)
        {
            return null;
        }

        return serviceAttribute.FindServiceTypeAutomatically ? ExtractServiceTypeFromInterfacesOrSelf(sourceType) : sourceType.GetCustomAttribute<ServiceAttribute>()?.ServiceType;
    }

    private static Type ExtractServiceTypeFromInterfacesOrSelf(Type sourceType)
    {
        return sourceType.GetInterfaces().FirstOrDefault() ?? sourceType;
    }
}