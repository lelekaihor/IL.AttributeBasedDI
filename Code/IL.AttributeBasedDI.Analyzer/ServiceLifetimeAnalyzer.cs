using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;

namespace IL.AttributeBasedDI.ServiceLifetimeAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ServiceLifetimeAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "DI1001";
    private static readonly LocalizableString Title = "Services Lifetime Mismatch";

    private static readonly LocalizableString Rule1MessageFormat = "The service '{0}' is registered with a higher lifetime scope than it's dependency '{1}', which is either directly registered with a lower lifetime scope or is an interface whose implementation has a lower lifetime scope. This discrepancy in service lifetimes may lead to unintended behavior during runtime.";
    private static readonly DiagnosticDescriptor Rule1 = new(DiagnosticId, Title, Rule1MessageFormat, "Usage", DiagnosticSeverity.Warning, isEnabledByDefault: true);
    
    private static readonly LocalizableString Rule2MessageFormat = "The service '{0}' is registered with lifetime scope that is different from other services implementing '{1}'. This discrepancy in service lifetimes may lead to unintended behavior during runtime.";
    private static readonly DiagnosticDescriptor Rule2 = new(DiagnosticId, Title, Rule2MessageFormat, "Usage", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule1, Rule2);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeClass, SymbolKind.NamedType);
    }

    private static void AnalyzeClass(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        var serviceAttribute = namedTypeSymbol
            .GetAttributes()
            .FirstOrDefault(ServiceAttributeSearchExpression);

        if (serviceAttribute == null) return;

        var classesWithInterfaces = Enumerable.Empty<INamedTypeSymbol>().ToImmutableList();
        if (namedTypeSymbol.Interfaces.Any() || namedTypeSymbol.Constructors.Any(x => x.Parameters.Any()))
        {
            classesWithInterfaces = GetAllTypesInNamespace(context.Compilation.GlobalNamespace)
                .Where(x => x.Interfaces.Any())
                .ToImmutableList();
        }

        var classLifetime = GetClassLifetimeFromAttribute(serviceAttribute);
        FindAndReportMissmatchesInDependentServicesLifetime(context, namedTypeSymbol, classesWithInterfaces, classLifetime);
        FindAndReportMissmatchesInSiblingServicesLifetime(context, namedTypeSymbol, classesWithInterfaces, classLifetime, serviceAttribute);
    }

    private static void FindAndReportMissmatchesInSiblingServicesLifetime(SymbolAnalysisContext context,
        INamedTypeSymbol namedTypeSymbol,
        ImmutableList<INamedTypeSymbol> classesWithInterfaces,
        int classLifetime,
        AttributeData serviceAttribute)
    {
        if (!namedTypeSymbol.Interfaces.Any())
        {
            return;
        }

        var interfaceClassRegisteredFor = ExtractInterfaceClassIsRegisteredFor(namedTypeSymbol, serviceAttribute);
        var otherClassesImplementingThisInterfaceHaveLifetimeMissmatches = FindImplementations(interfaceClassRegisteredFor, classesWithInterfaces)
            .Where(x => !namedTypeSymbol.Equals(x, SymbolEqualityComparer.Default))
            .Where(x => x.GetAttributes().Any(ServiceAttributeSearchExpression))
            .Where(x =>
            {
                var attribute = x.GetAttributes().First(ServiceAttributeSearchExpression);
                var interfaceSiblingRegisteredFor = ExtractInterfaceClassIsRegisteredFor(x, attribute);
                return interfaceSiblingRegisteredFor.Equals(interfaceSiblingRegisteredFor, SymbolEqualityComparer.Default);
            })
            .Any(x =>
            {
                var attribute = x.GetAttributes().First(ServiceAttributeSearchExpression);
                return classLifetime != GetClassLifetimeFromAttribute(attribute);
            });
        if (otherClassesImplementingThisInterfaceHaveLifetimeMissmatches)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule2, namedTypeSymbol.Locations[0], namedTypeSymbol.Name, interfaceClassRegisteredFor.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
        }
    }

    private static INamedTypeSymbol ExtractInterfaceClassIsRegisteredFor(INamedTypeSymbol namedTypeSymbol, AttributeData serviceAttribute)
    {
        var value1 = serviceAttribute.ConstructorArguments
            .FirstOrDefault(x => x.Type?.Name.Equals("ServiceType", StringComparison.InvariantCultureIgnoreCase) ?? false)
            .Value as INamedTypeSymbol;
        var value2 = serviceAttribute
            .NamedArguments
            .FirstOrDefault(x => x.Key.Equals("ServiceType", StringComparison.InvariantCultureIgnoreCase))
            .Value
            .Value as INamedTypeSymbol;
        return value1 ?? value2 ?? namedTypeSymbol.Interfaces.First();
    }

    private static void FindAndReportMissmatchesInDependentServicesLifetime(SymbolAnalysisContext context,
        INamedTypeSymbol namedTypeSymbol,
        ImmutableList<INamedTypeSymbol> classesWithInterfaces,
        int classLifetime)
    {
        foreach (var diagnostic in namedTypeSymbol
                     .Constructors
                     .SelectMany(constructor => constructor
                         .Parameters
                         .Select(parameter => parameter.Type as INamedTypeSymbol)
                         .Where(parameterType => parameterType != default)
                         .Select(x => new
                             {
                                 OriginalType = x,
                                 Implementations = x!.TypeKind == TypeKind.Interface ? FindImplementations(x, classesWithInterfaces) : new[] { x }
                             }
                         )
                         .Where(searchableObject => searchableObject.Implementations.Any(parameterType => parameterType!.GetAttributes().Any(ServiceAttributeSearchExpression)))
                         .Select(searchableObject => new
                         {
                             searchableObject.OriginalType,
                             parameterServiceAttribute = searchableObject
                                 .Implementations
                                 .First(parameterType => parameterType!.GetAttributes().Any(ServiceAttributeSearchExpression))
                                 .GetAttributes()
                                 .First(ServiceAttributeSearchExpression)
                         })
                         .Select(t => new { t, parameterLifetime = GetClassLifetimeFromAttribute(t.parameterServiceAttribute!) })
                         .Where(t => classLifetime > t.parameterLifetime)
                         .Select(t => Diagnostic.Create(Rule1, constructor.Locations[0], namedTypeSymbol.Name, t.t.OriginalType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))))
        {
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static IEnumerable<INamedTypeSymbol> FindImplementations(INamedTypeSymbol interfaceSymbol, ImmutableList<INamedTypeSymbol> classesWithInterfaces)
    {
        return classesWithInterfaces
            .Where(x => x.TypeKind == TypeKind.Class
                        && x.AllInterfaces.Contains(interfaceSymbol, SymbolEqualityComparer.Default)
                        && x.GetAttributes().Any(ServiceAttributeSearchExpression));
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypesInNamespace(INamespaceSymbol rootNamespace)
    {
        foreach (var type in rootNamespace.GetTypeMembers())
        {
            yield return type;
        }

        foreach (var subNamespace in rootNamespace.GetNamespaceMembers())
        {
            foreach (var type in GetAllTypesInNamespace(subNamespace))
            {
                yield return type;
            }
        }
    }

    private static bool ServiceAttributeSearchExpression(AttributeData attr)
    {
        return attr.AttributeClass?.Name is "ServiceAttribute" or "ServiceAttributeWithOptions";
    }

    private static int GetClassLifetimeFromAttribute(AttributeData serviceAttribute)
    {
        var value1 = (int)(serviceAttribute.ConstructorArguments.FirstOrDefault(x => x.Type?.Name.Equals("Lifetime", StringComparison.InvariantCultureIgnoreCase) ?? false).Value ??
                           0);
        var value2 = (int)(serviceAttribute.NamedArguments.FirstOrDefault(x => x.Key.Equals("Lifetime", StringComparison.InvariantCultureIgnoreCase)).Value.Value ?? 0);
        var classLifetime = Math.Max(value1, value2);
        return classLifetime;
    }
}