﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace epj.RouteGenerator;

[Generator]
public class RouteGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
                (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax,
                (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .Where(c => c is not null);

        var compilation = context.CompilationProvider.Combine(syntaxProvider.Collect());

        context.RegisterSourceOutput(compilation, Execute);
    }

    private void Execute(SourceProductionContext context, (Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) compilationTuple)
    {
        var (compilation, classes) = compilationTuple;

        var attributeName = typeof(AutoRouteGenerationAttribute).FullName!;

        var attributeSymbol = compilation.GetTypeByMetadataName(attributeName);

        if (attributeSymbol is null)
        {
            // Stop the generator if no such attribute has been found
            return;
        }

        var classesWithAttributeData = GetAllClasses(compilation.GlobalNamespace)
            .Select(t => new { Class = t, AttributeData = t.GetAttributes().FirstOrDefault(ad => ad?.AttributeClass is not null && ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)) })
            .Where(t => t.AttributeData != null)
            .ToList();

        var classesWithAttribute = classesWithAttributeData.Select(t => t.Class).ToList();

        if (!classesWithAttribute.Any())
        {
            // Stop the generator if no classes with the attribute have been found
            return;
        }

        if (classesWithAttributeData.First().AttributeData.ConstructorArguments.FirstOrDefault().Value is not string suffix)
        {
            // Stop the generator if the suffix is null
            return;
        }

        var namespaceName = classesWithAttribute.First().ContainingNamespace.ToDisplayString();

        var routeClassDeclarationSyntaxList = classes.Where(c => c.Identifier.Text.EndsWith(suffix)).ToList();
        var routeNameList = routeClassDeclarationSyntaxList.Select(pageClass => pageClass.Identifier.Text).ToList();

        var routeClassDeclarationStrings = routeNameList.Select(page => $"public const string {page} = \"{page}\";");
        var routeClassDeclarationsString = string.Join("\n    ", routeClassDeclarationStrings);

        var source = $$"""
                       // <auto-generated/>
                       namespace {{namespaceName}};

                       public static class Routes
                       {
                           {{routeClassDeclarationsString}}

                           public static List<string> AllRoutes = new()
                           {
                               {{string.Join(",\n        ", routeNameList)}}
                           };
                       }
                       """;

        context.AddSource("Routes.g.cs", source);
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
            else if (member is INamedTypeSymbol classSymbol && classSymbol.TypeKind == TypeKind.Class)
            {
                yield return classSymbol;
            }
        }
    }
}
