using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace PipeEx.SourceGenerators;

/// <summary>
/// Generates the <c>PipeEx.TupleDestructuring</c> class. For every tuple arity from
/// <see cref="TupleArity.Min"/> to <see cref="TupleArity.Max"/> it emits an <c>I</c>
/// extension method that destructures the tuple into the arguments of a transform.
/// </summary>
/// <remarks>Replaces the former <c>PipeEx/TupleDestructuring.sh</c> shell script.</remarks>
[Generator(LanguageNames.CSharp)]
public sealed class TupleDestructuringGenerator : IIncrementalGenerator
{
    /// <summary>The generators are shipped in one shared assembly, so each one only emits into its target assembly.</summary>
    private const string TargetAssembly = "PipeEx";

    private const string HintName = "TupleDestructuring.g.cs";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (compilation, _) => compilation.AssemblyName);

        context.RegisterSourceOutput(assemblyName, static (productionContext, name) =>
        {
            if (name != TargetAssembly)
            {
                return;
            }

            productionContext.AddSource(HintName, SourceText.From(Build(), Encoding.UTF8));
        });
    }

    private static string Build()
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine("namespace PipeEx;");
        sb.AppendLine();
        sb.AppendLine("public static class TupleDestructuring");
        sb.AppendLine("{");

        for (var arity = TupleArity.Min; arity <= TupleArity.Max; arity++)
        {
            sb.Append(MethodTemplate
                .Replace("$ty$", TupleArity.TypeList(arity))
                .Replace("$tv$", TupleArity.ValueList(arity)));
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    // A leading blank line separates each method; the trailing newline keeps the closing
    // brace of one method on its own line above the next.
    private const string MethodTemplate = """

            /// <summary>
            /// Applies a transformation function to the source object.
            /// </summary>
            /// <typeparam name="TSource">The type of the source object.</typeparam>
            /// <typeparam name="TResult">The type of the result object.</typeparam>
            /// <param name="source">The source object tuple.</param>
            /// <param name="transform">The transformation function.</param>
            /// <returns>The result of the transformation.</returns>
            public static TResult I<$ty$, TResult>(this ($ty$) source, Func<$ty$, TResult> transform)
            {
                return transform($tv$);
            }

        """;
}
