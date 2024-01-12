using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace epj.RouteGenerator;

[Generator]
public class RouteGenerator : IIncrementalGenerator
{
    private readonly Regex _classNameRegex = new(Constants.ClassNameRegex);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
                (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax,
                (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .Where(c => c is not null);

        var compilation = context.CompilationProvider.Combine(syntaxProvider.Collect());

        context.RegisterPostInitializationOutput((ctx) => ctx.AddSource(
            Constants.AutoRoutesAttribute,
            BuildAutoRoutesAttribute()));

        context.RegisterPostInitializationOutput((ctx) => ctx.AddSource(
            Constants.ExtraRouteAttribute,
            BuildExtraRouteAttribute()));

        context.RegisterSourceOutput(compilation, Execute);
    }

    private void Execute(SourceProductionContext context, (Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) compilationTuple)
    {
        var (compilation, classes) = compilationTuple;

        var attributeAutoGenSymbol = compilation.GetTypeByMetadataName(Constants.AutoRoutesFullName);

        if (attributeAutoGenSymbol is null)
        {
            // Stop the generator if no such attribute has been found (shouldn't happen as it's defined in the same assembly)
            return;
        }

        var classWithAutoGenAttributeData = GetAllClasses(compilation.GlobalNamespace)
            .Select(t => new
            {
                Class = t,
                AttributeData = t
                    .GetAttributes()
                    .FirstOrDefault(ad => ad?.AttributeClass is not null && ad.AttributeClass.Equals(attributeAutoGenSymbol, SymbolEqualityComparer.Default))
            })
            .First(t => t.AttributeData != null);

        if (classWithAutoGenAttributeData?.Class is null)
        {
            // Stop the generator if no class with the attribute has been found
            return;
        }

        if (classWithAutoGenAttributeData.AttributeData.ConstructorArguments.FirstOrDefault().Value is not string suffix || string.IsNullOrWhiteSpace(suffix))
        {
            // Stop the generator if the suffix is null or an empty string
            context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        Constants.ARG001,
                        Constants.Error,
                        $"The {Constants.AutoRoutesAttribute} suffix parameter is required and may not be null or empty and the class name must be valid",
                        Constants.ErrorCategoryCompilation, DiagnosticSeverity.Error,
                        true),
                    null));

            return;
        }

        var namespaceName = classWithAutoGenAttributeData.Class.ContainingNamespace.ToDisplayString();

        var routeClassDeclarationSyntaxList = classes.Where(c => c.Identifier.Text.EndsWith(suffix)).ToList();
        var routeNameList = routeClassDeclarationSyntaxList.Select(pageClass => pageClass.Identifier.Text).ToList();

        AddExtraRoutes(context, compilation, routeNameList);

        var source = BuildSource(routeNameList, namespaceName);

        context.AddSource(Constants.RoutesGenFileName, source);
    }

    private void AddExtraRoutes(SourceProductionContext context, Compilation compilation, ICollection<string> routeNameList)
    {
        var attributeExtraRouteSymbol = compilation.GetTypeByMetadataName(Constants.ExtraRouteFullName);

        if (attributeExtraRouteSymbol is null)
        {
            return;
        }

        var classesWithExtraRouteAttributeData = GetAllClasses(compilation.GlobalNamespace)
            .Select(t => new
            {
                Class = t,
                AttributeData = t.GetAttributes().Where(ad =>
                    ad?.AttributeClass is not null &&
                    ad.AttributeClass.Equals(attributeExtraRouteSymbol, SymbolEqualityComparer.Default))
            })
            .Where(t => t.AttributeData.Any())
            .ToList();

        if (!classesWithExtraRouteAttributeData.Any())
        {
            return;
        }

        // Add all extra routes to the routeNameList
        foreach (var attributeData in classesWithExtraRouteAttributeData.SelectMany(classWithExtraRouteAttributeData => classWithExtraRouteAttributeData.AttributeData))
        {
            if (attributeData.ConstructorArguments.FirstOrDefault().Value is not string extraRoute || string.IsNullOrWhiteSpace(extraRoute))
            {
                continue;
            }

            //make sure route is valid and doesn't exist in routeNameList yet
            if (!_classNameRegex.IsMatch(extraRoute))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        Constants.EXR001,
                        Constants.Error,
                        $"The {Constants.ExtraRouteAttribute} route parameter must be a valid route name, ignoring invalid route '{extraRoute}'",
                        Constants.ErrorCategoryCompilation,
                        DiagnosticSeverity.Warning,
                        true),
                    null));

                continue;
            }

            if (routeNameList.Contains(extraRoute))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        Constants.EXR002,
                        Constants.Error,
                        $"The {Constants.ExtraRouteAttribute} route parameter must be unique, ignoring duplicate '{extraRoute}'",
                        Constants.ErrorCategoryCompilation,
                        DiagnosticSeverity.Warning,
                        true),
                    null));

                continue;
            }

            routeNameList.Add(extraRoute);
        }
    }

    private static string BuildSource(IReadOnlyCollection<string> routeNameList, string namespaceName)
    {
        var routeClassDeclarationStrings = routeNameList.Select(page => $"public const string {page} = \"{page}\";");
        var routeClassDeclarationsString = string.Join("\n        ", routeClassDeclarationStrings);

        var source = $$"""
                       // <auto-generated/>
                       using System.Collections.ObjectModel;

                       namespace {{namespaceName}}
                       {
                           public static class Routes
                           {
                               {{routeClassDeclarationsString}}
                           
                               private static List<string> allRoutes = new()
                               {
                                   {{string.Join(",\n            ", routeNameList)}}
                               };
                               
                               public static ReadOnlyCollection<string> AllRoutes => allRoutes.AsReadOnly();
                           }
                       }
                       """;
        return source;
    }

    // Helper method to get all classes in a namespace
    private static IEnumerable<INamedTypeSymbol> GetAllClasses(INamespaceSymbol namespaceSymbol)
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            if (member is INamespaceSymbol childNamespace)
            {
                foreach (var childClass in GetAllClasses(childNamespace))
                {
                    yield return childClass;
                }
            }
            else if (member is INamedTypeSymbol { TypeKind: TypeKind.Class } classSymbol)
            {
                yield return classSymbol;
            }
        }
    }

    private static string BuildAutoRoutesAttribute()
    {
        return """
               using System;

               namespace epj.RouteGenerator;

               [AttributeUsage(AttributeTargets.Class, Inherited = false)]
               public class AutoRoutesAttribute : Attribute
               {
                   public string Suffix { get; }
               
                   public AutoRoutesAttribute(string suffix)
                   {
                       Suffix = suffix;
                   }
               }
               """;
    }

    private static string BuildExtraRouteAttribute()
    {
        return """
               using System;
               
               namespace epj.RouteGenerator;
               
               [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
               public class ExtraRouteAttribute : Attribute
               {
                   public string Route { get; }
               
                   public ExtraRouteAttribute(string route)
                   {
                       Route = route;
                   }
               } 
               """;
    }
}
