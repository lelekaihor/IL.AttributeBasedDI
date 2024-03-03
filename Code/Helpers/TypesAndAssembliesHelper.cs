using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IL.AttributeBasedDI.Helpers;

public static class TypesAndAssembliesHelper
{
    public static Assembly[] GetAssemblies(string[] assemblyFilters)
    {
        var assemblyNames = new HashSet<string>(assemblyFilters.Where(filter => !filter.Contains('*')));
        var wildcardNames = assemblyFilters.Where(filter => filter.Contains('*')).ToArray();

        var allAssemblies = new HashSet<Assembly>();
        allAssemblies.UnionWith(
            Assembly
                .GetCallingAssembly()
                .GetReferencedAssemblies()
                .Select(Assembly.Load));
        allAssemblies.UnionWith(
            AppDomain
                .CurrentDomain
                .GetAssemblies()
        );

        var assemblies = allAssemblies
            .Where(assembly =>
            {
                if (!assemblyFilters.Any())
                {
                    return true;
                }

                var nameToMatch = assembly.GetName().Name!;
                return assemblyNames.Contains(nameToMatch) || wildcardNames.Any(wildcard => IsWildcardMatch(nameToMatch, wildcard));
            })
            .ToArray();

        return assemblies;
    }

    public static IEnumerable<Type> GetExportedTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetExportedTypes();
        }
        catch (NotSupportedException)
        {
            // A type load exception would typically happen on an Anonymously Hosted DynamicMethods
            // Assembly and it would be safe to skip this exception.
            return Type.EmptyTypes;
        }
        catch (FileLoadException)
        {
            // The assembly points to a not found assembly - ignore and continue
            return Type.EmptyTypes;
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Return the types that could be loaded. Types can contain null values.
            return ex.Types.Where(type => type != null)!;
        }
        catch (Exception ex)
        {
            // Throw a more descriptive message containing the name of the assembly.
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unable to load types from assembly {0}. {1}", assembly.FullName, ex.Message), ex);
        }
    }

    /// <summary>
    ///     Checks if a string matches a wildcard argument (using regex)
    /// </summary>
    private static bool IsWildcardMatch(string input, string wildcard)
    {
        return input == wildcard || Regex.IsMatch(input, $"^{Regex.Escape(wildcard).Replace("\\*", ".*").Replace("\\?", ".")}$", RegexOptions.IgnoreCase);
    }
}