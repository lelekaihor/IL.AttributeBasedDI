using System.Reflection;
using IL.AttributeBasedDI.Attributes;
using IL.AttributeBasedDI.Extensions;
using IL.Misc.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IL.AttributeBasedDI.Helpers;

internal static class ServiceRegistrationHelper
{
    public static void RegisterClassesWithServiceAttributeAndDecorators<TFeatureFlag>(this IServiceCollection serviceCollection,
        TFeatureFlag activeFeatures,
        IConfiguration? configuration = null,
        params string[] assemblyFilters) where TFeatureFlag : struct, Enum
    {
        var assemblies = TypesAndAssembliesHelper.GetAssemblies(assemblyFilters);
        var allTypes = GetAllTypesFromAssemblies(assemblies);
        serviceCollection.RegisterClassesWithServiceAttributes(activeFeatures, allTypes);
        serviceCollection.RegisterClassesWithServiceAttributesWithOptions(activeFeatures, configuration, allTypes);
        serviceCollection.RegisterClassesWithDecoratorAttributes(activeFeatures, allTypes);
    }

    public static Type? GetServiceTypeBasedOnDependencyInjectionAttribute<TFeatureFlag>(Type sourceType,
        DependencyInjectionAttributeBase<TFeatureFlag> dependencyInjectionAttributeBase)
        where TFeatureFlag : struct, Enum
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
            .Where(type => type is { IsAbstract: false })
            .ToArray();
    }
}