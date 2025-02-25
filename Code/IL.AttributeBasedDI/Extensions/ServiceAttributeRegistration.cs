using System.Reflection;
using System.Runtime.InteropServices;
using IL.AttributeBasedDI.Attributes;
using IL.AttributeBasedDI.Helpers;
using IL.AttributeBasedDI.Models;
using Microsoft.Extensions.DependencyInjection;

namespace IL.AttributeBasedDI.Extensions;

internal static class ServiceAttributeRegistration
{
    public static void RegisterClassesWithServiceAttributes(this IServiceCollection serviceCollection, params Type[] types)
    {
        var serviceRegistrations = types
            .Where(type => type.GetCustomAttributes<ServiceAttribute>().Any())
            .SelectMany(type =>
                type
                    .GetCustomAttributes<ServiceAttribute>()
                    .Select(attribute => ToRegistrationEntry(attribute, type))
            )
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

    public static RegistrationEntry ToRegistrationEntry(this ServiceAttribute attribute, Type type)
    {
        return new RegistrationEntry
        {
#if NET8_0_OR_GREATER
            Key = attribute.Key,
#endif
            ServiceLifetime = attribute.Lifetime,
            ServiceType = ServiceRegistrationHelper.GetServiceTypeBasedOnDependencyInjectionAttribute(type, attribute),
            ImplementationType = type
        };
    }

    public static void AddServiceWithLifetime(this IServiceCollection serviceCollection, Type type, Lifetime lifetime, string? key)
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

    public static void AddServiceWithLifetime(this IServiceCollection serviceCollection, Type serviceType, Type implementationType, Lifetime lifetime, string? key)
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
}