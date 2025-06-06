﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace epj.RouteGenerator;

[Generator]
public class RouteGenerator : IIncrementalGenerator
{
    private static readonly Regex ClassNameRegex = new(Constants.ClassNameRegex);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
                static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax,
                static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .Where(static c => c is not null);

        var compilation = context.CompilationProvider.Combine(syntaxProvider.Collect());

        context.RegisterPostInitializationOutput((ctx) => ctx.AddSource(
            Constants.AutoRoutesAttribute,
            BuildAutoRoutesAttribute()));

        context.RegisterPostInitializationOutput((ctx) => ctx.AddSource(
            Constants.ExtraRouteAttribute,
            BuildExtraRouteAttribute()));

        context.RegisterPostInitializationOutput((ctx) => ctx.AddSource(
            Constants.IgnoreRouteAttribute,
            BuildIgnoreRouteAttribute()));

        context.RegisterSourceOutput(compilation, Execute);
    }

    private static void Execute(SourceProductionContext context, (Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) compilationTuple)
    {
        try
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
                .FirstOrDefault(t => t.AttributeData != null);

            if (classWithAutoGenAttributeData?.Class is null)
            {
                // Stop the generator if no class with the attribute has been found
                return;
            }

            if (classWithAutoGenAttributeData.AttributeData.ConstructorArguments.FirstOrDefault().Value is not string suffix || string.IsNullOrWhiteSpace(suffix))
            {
                // Stop the generator if the suffix is null or an empty string
                context.ReportDiagnostic(CreateDiagnostic(
                    Constants.ARG001,
                    Constants.Error,
                    $"The {Constants.AutoRoutesAttribute} suffix parameter is required and may not be null or empty and the class name must be valid",
                    DiagnosticSeverity.Error));

                return;
            }

            var namespaceName = classWithAutoGenAttributeData.Class.ContainingNamespace.ToDisplayString();

            // Get all non-abstract classes with the specified suffix and without the IgnoreRoute attribute
            var routeClassDeclarationSyntaxList = classes
                .Where(c => c.Identifier.Text.EndsWith(suffix) &&
                       !c.AttributeLists
                           .SelectMany(al => al.Attributes)
                           .Any(a => a.Name.ToString().Equals(Constants.IgnoreRouteName)) &&
                       !c.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
                .ToList();

            var routeNameList = routeClassDeclarationSyntaxList.Select(pageClass => pageClass.Identifier.Text)
                .Distinct()
                .ToList();

            var routesAndTypeNamesDictionary = new Dictionary<string, string>();

            foreach (var route in routeNameList)
            {
                var routeClass = classes.FirstOrDefault(c => c.Identifier.Text == route);
                var semanticModel = compilation.GetSemanticModel(routeClass.SyntaxTree);
                var classSymbol = ModelExtensions.GetDeclaredSymbol(semanticModel, routeClass);
                var routeTypename = $"{classSymbol!.ContainingNamespace}.{route}";
                routesAndTypeNamesDictionary.Add(route, routeTypename);
            }

            AddExtraRoutes(context, compilation, classes, routeNameList, routesAndTypeNamesDictionary);

            var source = BuildSource(routeNameList, routesAndTypeNamesDictionary, namespaceName);

            context.AddSource(Constants.RoutesGenFileName, source);
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(CreateDiagnostic(
                Constants.RGE001,
                Constants.Warning,
                $"An unexpected error occurred during source generation: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n. The generator output may be invalid.",
                DiagnosticSeverity.Warning));
        }
    }

    private static void AddExtraRoutes(SourceProductionContext context, Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, ICollection<string> routeNameList, Dictionary<string, string> routesAndTypeNamesDictionary)
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
            if (!ClassNameRegex.IsMatch(extraRoute))
            {
                context.ReportDiagnostic(CreateDiagnostic(
                    Constants.EXR001,
                    Constants.Error,
                    $"The {Constants.ExtraRouteAttribute} route parameter must be a valid route name, ignoring invalid route '{extraRoute}'",
                    DiagnosticSeverity.Warning));

                continue;
            }

            if (routeNameList.Contains(extraRoute))
            {
                context.ReportDiagnostic(CreateDiagnostic(
                    Constants.EXR002,
                    Constants.Error,
                    $"The {Constants.ExtraRouteAttribute} route parameter must be unique, ignoring duplicate '{extraRoute}'",
                    DiagnosticSeverity.Warning));

                continue;
            }

            routeNameList.Add(extraRoute);

            // if the attribute has a Type parameter, get the FullName of the Type and store it in a variable
            if (attributeData.ConstructorArguments.Length > 1 &&
                attributeData.ConstructorArguments[1].Value is INamedTypeSymbol typeSymbol)
            {
                var typeName = typeSymbol.ToString();
                routesAndTypeNamesDictionary.Add(extraRoute, typeName);

                // typename for route already found, no need to check further
                continue;
            }

            var extraRouteClass = classes.FirstOrDefault(c => c.Identifier.Text == extraRoute);

            if (extraRouteClass is null)
            {
                // no class found for route
                context.ReportDiagnostic(CreateDiagnostic(
                    Constants.EXR003,
                    Constants.Warning,
                    $"No class or typename found for {Constants.ExtraRouteAttribute}: '{extraRoute}'. Please specify the type.",
                    DiagnosticSeverity.Warning));

                continue;
            }

            var semanticModel = compilation.GetSemanticModel(extraRouteClass.SyntaxTree);
            var classSymbol = ModelExtensions.GetDeclaredSymbol(semanticModel, extraRouteClass);
            var extraRouteTypename = $"{classSymbol!.ContainingNamespace}.{extraRoute}";
            routesAndTypeNamesDictionary.Add(extraRoute, extraRouteTypename);
        }
    }

    private static string BuildSource(IReadOnlyCollection<string> routeNameList, Dictionary<string, string> routesAndNamespacesDictionary, string namespaceName)
    {
        var routeClassDeclarationStrings = routeNameList.Select(page => $"public const string {page} = \"{page}\";");
        var routeClassDeclarationsString = string.Join("\n    ", routeClassDeclarationStrings);

        var source = $$"""
                       // <auto-generated/>
                       using System;
                       using System.Collections.Generic;
                       using System.Collections.ObjectModel;

                       namespace {{namespaceName}};
                       
                       public static class Routes
                       {
                           {{routeClassDeclarationsString}}
                       
                           private static List<string> allRoutes = new()
                           {
                               {{string.Join(",\n        ", routeNameList)}}
                           };
                           
                           public static ReadOnlyCollection<string> AllRoutes => allRoutes.AsReadOnly();
                           
                           private static Dictionary<string, Type> routeTypeMap = new()
                           {
                               {{string.Join(",\n        ", routesAndNamespacesDictionary.Select(x => $"{{ {x.Key}, typeof({x.Value}) }}"))}}
                           };
                           
                           public static ReadOnlyDictionary<string, Type> RouteTypeMap => routeTypeMap.AsReadOnly();
                       }
                       """;
        return source;
    }

    // Helper method to get all classes in a namespace
    private static IEnumerable<INamedTypeSymbol> GetAllClasses(INamespaceSymbol namespaceSymbol)
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            switch (member)
            {
                case INamespaceSymbol childNamespace:
                {
                    foreach (var childClass in GetAllClasses(childNamespace))
                    {
                        yield return childClass;
                    }
                    break;
                }
                case INamedTypeSymbol { TypeKind: TypeKind.Class } classSymbol:
                    yield return classSymbol;
                    break;
            }
        }
    }

    private static string BuildAutoRoutesAttribute()
    {
        return """
               // <auto-generated/>
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
               // <auto-generated/>
               #nullable enable
               
               using System;
               
               namespace epj.RouteGenerator;
               
               [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
               public class ExtraRouteAttribute : Attribute
               {
                   public string Route { get; }
                   public Type? Typename { get; }
               
                   public ExtraRouteAttribute(string route)
                   {
                       Route = route;
                   }
               
                   public ExtraRouteAttribute(string route, Type typename)
                   {
                       Route = route;
                       Typename = typename;
                   }
               } 
               
               #nullable disable
               """;
    }

    private static string BuildIgnoreRouteAttribute()
    {
        return """
               // <auto-generated/>
               using System;

               namespace epj.RouteGenerator;

               [AttributeUsage(AttributeTargets.Class, Inherited = false)]
               public class IgnoreRouteAttribute : Attribute
               {
               }
               """;
    }

    private static Diagnostic CreateDiagnostic(string id, string title, string message, DiagnosticSeverity severity, Location location = null)
    {
        return Diagnostic.Create(
            new DiagnosticDescriptor(
                id,
                title,
                message,
                Constants.ErrorCategoryCompilation,
                severity,
                true),
            location ?? Location.None);
    }
}
