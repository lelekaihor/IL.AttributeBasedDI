using IL.AttributeBasedDI.Models;
using IL.Misc.Helpers;

namespace IL.AttributeBasedDI.Extensions;

public static class ServiceGraphExtensions
{
        internal static void AddOrMerge<TFeatureFlag>(this ServiceGraph serviceGraph, RegistrationEntry<TFeatureFlag> registrationEntry) where TFeatureFlag : struct, Enum
        {
            try
            {
                if (registrationEntry.ServiceType == null)
                {
                    return;
                }

                // Get the services group for this service type
                var serviceTypeGroup = serviceGraph.GetServicesOfType(registrationEntry.ServiceType);

                // Try to find an existing service match
                var existing = serviceTypeGroup.FirstOrDefault(service =>
                    service.ImplementationType == registrationEntry.ImplementationType &&
                    service.Key == registrationEntry.Key &&
                    service.Lifetime == registrationEntry.Lifetime);

                var featureEnum = registrationEntry.Feature as Enum;

                if (existing is not null)
                {
                    existing.Features.Add(featureEnum);
                }
                else
                {
                    var newService = new ServiceNode
                    {
                        ServiceType = registrationEntry.ServiceType,
                        ImplementationType = registrationEntry.ImplementationType,
                        Key = registrationEntry.Key,
                        Lifetime = registrationEntry.Lifetime,
                        Features = [featureEnum]
                    };

                    serviceGraph.AddServiceToGroup(newService);
                }
            }
            catch
            {
                //do nothing about
            }
        }
        
        /// <summary>
        /// Adds a service to the appropriate type group in the dictionary
        /// </summary>
        private static void AddServiceToGroup(this ServiceGraph serviceGraph, ServiceNode service)
        {
            if (service.ServiceType == null)
            {
                return;
            }
            
            if (!serviceGraph.ServicesByType.TryGetValue(service.ServiceType, out var servicesOfType))
            {
                servicesOfType = [];
                serviceGraph.ServicesByType[service.ServiceType] = servicesOfType;
            }
            
            servicesOfType.Add(service);
        }
        
        /// <summary>
        /// Gets all services of a specific type
        /// </summary>
        private static List<ServiceNode> GetServicesOfType(this ServiceGraph serviceGraph, Type serviceType)
        {
            if (!serviceGraph.ServicesByType.TryGetValue(serviceType, out var servicesOfType))
            {
                servicesOfType = [];
                serviceGraph.ServicesByType[serviceType] = servicesOfType;
            }
            
            return servicesOfType;
        }
        
        internal static void AddDecorator<TFeatureFlag>(this ServiceGraph serviceGraph, Type serviceType, Type decoratorImplementationType, string? key, TFeatureFlag feature, bool treatOpenGenericsAsWildcard = false) where TFeatureFlag : struct, Enum
        {
            try
            {
                var featureEnum = feature as Enum;
            
                // Find services matching the type and key
                var matchingServices = serviceGraph.FindMatchingServices(serviceType, key, treatOpenGenericsAsWildcard);
            
                if (matchingServices.Count == 0)
                {
                    // If no services match, create a new service node with just the decorator
                    // This will be linked to actual implementations when they are registered
                    var newService = new ServiceNode
                    {
                        ServiceType = serviceType,
                        Key = key,
                        Decorators = [decoratorImplementationType],
                        Features = [featureEnum]
                    };
                
                    serviceGraph.AddServiceToGroup(newService);
                    return;
                }
            
                // Add the decorator to all matching services
                foreach (var service in matchingServices)
                {
                    if (!service.Decorators.Contains(decoratorImplementationType))
                    {
                        service.Decorators.Add(decoratorImplementationType);
                    }
                
                    // Also add the feature flag if not present
                    service.Features.Add(featureEnum);
                }
            }
            catch
            {
                //do nothing about
            }
            
        }
        
        /// <summary>
        /// Finds services that match the specified service type and key, with options for open generic handling
        /// </summary>
        private static List<ServiceNode> FindMatchingServices(this ServiceGraph serviceGraph, Type serviceType, string? key, bool treatOpenGenericsAsWildcard)
        {
            if (!treatOpenGenericsAsWildcard || !serviceType.IsGenericType)
            {
                // Standard matching by exact type and key
                if (serviceGraph.ServicesByType.TryGetValue(serviceType, out var servicesOfType))
                {
                    return servicesOfType.Where(service => key == null || service.Key == key || service.Key?.MatchesWildcard(key) is true).ToList();
                }
                
                return [];
            }
            
            // For open generics with wildcard matching, find all services that implement
            // any version of this generic interface
            var serviceTypeFullName = serviceType.FullName;
            if (string.IsNullOrEmpty(serviceTypeFullName))
            {
                return [];
            }
            
            if (!string.IsNullOrEmpty(key))
            {
                // Currently not supporting wildcard open generics for keyed services
                if (serviceGraph.ServicesByType.TryGetValue(serviceType, out var servicesOfType))
                {
                    return servicesOfType.Where(service => service.Key == key || service.Key?.MatchesWildcard(key) is true).ToList();
                }
                
                return [];
            }
            
            // For wildcard generic matching, we need to scan all service types
            var baseServiceTypeName = serviceTypeFullName.Split('`')[0];
            return serviceGraph
                .ServicesByType
                .Where(kv => 
                    kv.Key.IsGenericType && 
                    kv.Key.FullName?.StartsWith(baseServiceTypeName) == true)
                .SelectMany(kv => kv.Value)
                .ToList();
        }
}