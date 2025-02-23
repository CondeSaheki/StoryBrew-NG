using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace StoryBrew.Generator;

[Generator]
public class EntryPoint : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        const string name = "StoryBrew.Storyboard.Script";

        var scriptClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (context, _) =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)context.Node;
                    var model = context.SemanticModel;
                    if (model.GetDeclaredSymbol(classDeclaration, _) is not INamedTypeSymbol typeSymbol) return null;
                    
                    return hasInherit(typeSymbol, name) ? typeSymbol.ToDisplayString() : null;
                })
            .Where(typeName => typeName != null)
            .Collect();

        context.RegisterSourceOutput(scriptClasses, (context, scriptList) =>
        {
            var arrayBuilder = new StringBuilder();
            foreach (var type in scriptList) arrayBuilder.AppendLine($"typeof({type}),");

            var switchBuilder = new StringBuilder();
            foreach (var type in scriptList) switchBuilder.AppendLine($"$\"{type}\" => typeof({type}),");

            var BuildId = Guid.NewGuid().ToString();
            
            var text =
            $$"""
            #nullable enable

            public static class Program
            {
                private const string buildID = "{{BuildId}}";

                private static ReadOnlySpan<char> getBuildId() => buildID;

                private static readonly Type[] scripts = 
                [
                    {{arrayBuilder}}
                ];

                private static ReadOnlySpan<Type> getScripts() => scripts;

                private static Type? getScriptType(ReadOnlySpan<char> name) => name switch
                {
                    {{switchBuilder}}
                    _ => null
                };

                public static void Main(string[] args)
                {
                    new StoryBrew.Runtime.Bootstrap(getScripts, getScriptType, getBuildId).Arguments(args);
                }
            }
            """;

            context.AddSource("Program.g.cs", SourceText.From(text, Encoding.UTF8));
        });
    }

    private static bool hasInherit(in INamedTypeSymbol type, ReadOnlySpan<char> name)
    {
        for (var symbol = type.BaseType; symbol != null; symbol = symbol.BaseType)
        {
            if (symbol.ToDisplayString().AsSpan().SequenceEqual(name)) return true;
        }
        return false;
    }
}