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
            .Where(c => c is not null && c.Identifier.Text.EndsWith("Page"));

        var compilation = context.CompilationProvider.Combine(syntaxProvider.Collect());

        context.RegisterSourceOutput(compilation, Execute);
    }

    private void Execute(SourceProductionContext context, (Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) compilationTuple)
    {
        var (compilation, pageClasses) = compilationTuple;

        var list = new List<string>();

        foreach (var pageClass in pageClasses)
        {
            list.Add(pageClass.Identifier.Text);
        }

        var classDeclarations = list.Select(page => $"public const string {page} = \"{page}\";");
        var classDeclarationsString = string.Join("\n    ", classDeclarations);

        var source = $$"""
                       // <auto-generated/>
                       //namespace {{compilation.GetEntryPoint(context.CancellationToken)?.ContainingNamespace.ToDisplayString()}};

                       public static class Routes
                       {
                           {{classDeclarationsString}}

                           public static List<string> AllRoutes = new()
                           {
                               {{string.Join(",\n        ", list)}}
                           };
                       }
                       """;

        context.AddSource("Routes.g.cs", source);
    }
}
