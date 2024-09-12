using System.Reflection;
using System.Runtime.InteropServices;
using IL.AttributeBasedDI.Attributes;
using IL.Misc.Helpers;
using Microsoft.Extensions.Configuration;
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
            serviceCollection.RegisterClassesWithServiceAttributeAndDecorators(default, solutionItemWildcard);
        }
    }
    
    public static void AddServiceAttributeBasedDependencyInjectionWithOptions(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddServiceAttributeBasedDependencyInjectionWithOptions(configuration, "*");
    }
    
    public static void AddServiceAttributeBasedDependencyInjectionWithOptions(this IServiceCollection serviceCollection, IConfiguration configuration, params string[] assemblyFilters)
    {
        foreach (var solutionItemWildcard in assemblyFilters.AsSpan())
        {
            serviceCollection.RegisterClassesWithServiceAttributeAndDecorators(configuration, solutionItemWildcard);
        }
    }

    private static void RegisterClassesWithServiceAttributeAndDecorators(this IServiceCollection serviceCollection, IConfiguration? configuration = null, params string[] assemblyFilters)
    {
        var assemblies = TypesAndAssembliesHelper.GetAssemblies(assemblyFilters);
        var allTypes = GetAllTypesFromAssemblies(assemblies);
        serviceCollection.RegisterClassesWithServiceAttributeInAssemblies(allTypes);
#if NET7_0_OR_GREATER
        serviceCollection.RegisterClassesWithServiceAttributeWithOptionsInAssemblies(configuration, allTypes);
#endif
        serviceCollection.RegisterClassesWithDecoratorAttributeInAssemblies(allTypes);
    }

    #region ServiceAttribute

    private static void RegisterClassesWithServiceAttributeInAssemblies(this IServiceCollection serviceCollection, params Type[] types)
    {
        var serviceRegistrations = types
            .Where(type => type.GetCustomAttribute<ServiceAttribute>() != default)
            .Select(type =>
            {
                var attribute = type.GetCustomAttribute<ServiceAttribute>()!;
                return new
                {
#if NET8_0_OR_GREATER
                    attribute.Key,
#endif
                    ServiceLifetime = attribute.Lifetime,
                    ServiceType = GetServiceTypeBasedOnDependencyInjectionAttribute(type, attribute),
                    ImplementationType = type
                };
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
#if NET7_0_OR_GREATER
    private static void RegisterClassesWithServiceAttributeWithOptionsInAssemblies(this IServiceCollection serviceCollection, IConfiguration? configuration = null, params Type[] types)
    {
        var serviceRegistrations = types
            .Where(type => type.GetCustomAttribute(typeof(ServiceWithOptionsAttribute<>)) != default)
            .Select(type =>
            {
                var attribute = type.GetCustomAttribute(typeof(ServiceWithOptionsAttribute<>));
                var configurationPathBase = attribute as IAttributeWithOptionsConfigurationPath;
                var configurationPath = configurationPathBase!.ConfigurationPath ?? string.Empty;
                var genericTypeUsedOnAttributeDeclaration = attribute!.GetType().GetGenericArguments().First();
                var instance = Activator.CreateInstance(genericTypeUsedOnAttributeDeclaration);
                if (!string.IsNullOrEmpty(configurationPath) && configuration != default)
                {
                    configuration.GetSection(configurationPath).Bind(instance);
                }
                serviceCollection.AddSingleton(genericTypeUsedOnAttributeDeclaration, instance!);

                var @base = attribute as ServiceAttribute;
                return new
                {
#if NET8_0_OR_GREATER
                    @base!.Key,
#endif
                    ServiceLifetime = @base!.Lifetime,
                    ServiceType = GetServiceTypeBasedOnDependencyInjectionAttribute(type, @base),
                    ImplementationType = type
                };
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

#endif
    private static void AddServiceWithLifetime(this IServiceCollection serviceCollection, Type type, Lifetime lifetime, string? key)
    {
        switch (lifetime)
        {
            case Lifetime.Singleton:
#if NET8_0_OR_GREATER
                if (!string.IsNullOrEmpty(key))
                {
                    serviceCollection.AddKeyedSingleton(serviceType: type, key);
                    break;
                }
#endif
                serviceCollection.AddSingleton(type);
                break;

            case Lifetime.Transient:
#if NET8_0_OR_GREATER
                if (!string.IsNullOrEmpty(key))
                {
                    serviceCollection.AddKeyedTransient(type, key);
                    break;
                }
#endif
                serviceCollection.AddTransient(type);
                break;

            case Lifetime.Scoped:
#if NET8_0_OR_GREATER
                if (!string.IsNullOrEmpty(key))
                {
                    serviceCollection.AddKeyedScoped(type, key);
                    break;
                }
#endif
                serviceCollection.AddScoped(type);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
    }

    private static void AddServiceWithLifetime(this IServiceCollection serviceCollection, Type serviceType, Type implementationType, Lifetime lifetime, string? key)
    {
        switch (lifetime)
        {
            case Lifetime.Singleton:
#if NET8_0_OR_GREATER
                if (!string.IsNullOrEmpty(key))
                {
                    serviceCollection.AddKeyedSingleton(serviceType, key, implementationType);
                    break;
                }
#endif
                serviceCollection.AddSingleton(serviceType, implementationType);
                break;

            case Lifetime.Transient:
#if NET8_0_OR_GREATER
                if (!string.IsNullOrEmpty(key))
                {
                    serviceCollection.AddKeyedTransient(serviceType, key, implementationType);
                    break;
                }
#endif
                serviceCollection.AddTransient(serviceType, implementationType);
                break;

            case Lifetime.Scoped:
#if NET8_0_OR_GREATER
                if (!string.IsNullOrEmpty(key))
                {
                    serviceCollection.AddKeyedScoped(serviceType, key, implementationType);
                    break;
                }
#endif
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
            .Select(type =>
            {
                var decoratorAttribute = type.GetCustomAttribute<DecoratorAttribute>()!;
                return new
                {
#if NET8_0_OR_GREATER
                    decoratorAttribute.Key,
#endif
                    decoratorAttribute.DecorationOrder,
                    ServiceType = GetServiceTypeBasedOnDependencyInjectionAttribute(type, decoratorAttribute),
                    DecoratorImplementationType = type
                };
            })
            .OrderBy(x => x.DecorationOrder)
            .ToList();

        foreach (var serviceDecorationEntry in CollectionsMarshal.AsSpan(serviceDecorations))
        {
            if (serviceDecorationEntry.ServiceType == null)
            {
                throw new InvalidOperationException(
                    $"Can't determine service to decorate. Decorator type: {serviceDecorationEntry.DecoratorImplementationType.FullName}");
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
    private static void AddDecoratorForService(this IServiceCollection serviceCollection, Type serviceType, Type decoratorImplementationType, string? key)
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
            throw new InvalidOperationException(
                $"No services registered for type {serviceType} in ServiceCollection, Decoration is impossible.");
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

    private static object CreateInstance(this IServiceProvider serviceProvider, ServiceDescriptor serviceDescriptor)
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

    #endregion DecoratorAttribute


    private static Type? GetServiceTypeBasedOnDependencyInjectionAttribute<T>(Type sourceType, T dependencyInjectionAttributeBase) where T : DependencyInjectionAttributeBase
    {
        return dependencyInjectionAttributeBase.FindServiceTypeAutomatically
            ? ExtractServiceTypeFromInterfaces(sourceType)
            : dependencyInjectionAttributeBase.ServiceType;
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