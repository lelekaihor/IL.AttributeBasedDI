using System.Reflection;
using IL.AttributeBasedDI.Attributes;
using IL.AttributeBasedDI.Extensions;
using IL.Misc.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IL.AttributeBasedDI.Helpers;

internal static class ServiceRegistrationHelper
{
    public static void RegisterClassesWithServiceAttributeAndDecorators(this IServiceCollection serviceCollection, IConfiguration? configuration = null,
        params string[] assemblyFilters)
    {
        var assemblies = TypesAndAssembliesHelper.GetAssemblies(assemblyFilters);
        var allTypes = GetAllTypesFromAssemblies(assemblies);
        serviceCollection.RegisterClassesWithServiceAttributes(allTypes);
#if NET7_0_OR_GREATER
        serviceCollection.RegisterClassesWithServiceAttributesWithOptions(configuration, allTypes);
#endif
        serviceCollection.RegisterClassesWithDecoratorAttributes(allTypes);
    }

    public static Type? GetServiceTypeBasedOnDependencyInjectionAttribute<T>(Type sourceType, T dependencyInjectionAttributeBase) where T : DependencyInjectionAttributeBase
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