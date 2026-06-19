using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace PipeEx.SourceGenerators;

/// <summary>
/// Emits the tuple-arity <c>I</c> overloads that were previously produced by the committed
/// <c>*.TupleDestructuring.g.cs</c> files and their generating shell scripts. The same generator is
/// referenced by both the <c>PipeEx</c> and <c>PipeEx.StructuredConcurrency</c> projects; it keys off the
/// compilation's assembly name so each assembly receives only the class that belongs to it:
/// <list type="bullet">
/// <item><c>PipeEx</c> gets the synchronous tuple-destructuring <c>I</c> overloads.</item>
/// <item><c>PipeEx.StructuredConcurrency</c> gets the eight StructuredTask <c>I</c> overloads per arity.</item>
/// </list>
/// Tuples carry 2..13 elements, matching the original scripts (<c>seq 1 12</c>, arity = item + 1).
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class TupleDestructuringGenerator : IIncrementalGenerator
{
    private const int MinArity = 2;
    private const int MaxArity = 13;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (compilation, _) => compilation.AssemblyName);

        context.RegisterSourceOutput(assemblyName, static (spc, name) =>
        {
            switch (name)
            {
                case "PipeEx":
                    spc.AddSource("TupleDestructuring.g.cs", SourceText.From(BuildPipeEx(), Encoding.UTF8));
                    break;
                case "PipeEx.StructuredConcurrency":
                    spc.AddSource("StructuredConcurrency.TupleDestructuring.g.cs", SourceText.From(BuildStructuredConcurrency(), Encoding.UTF8));
                    break;
            }
        });
    }

    private static string BuildPipeEx()
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace PipeEx;");
        sb.AppendLine();
        sb.AppendLine("public static class TupleDestructuring");
        sb.AppendLine("{");

        for (var arity = MinArity; arity <= MaxArity; arity++)
        {
            var ty = TypeParams(arity);
            var tv = Items(arity, "source");

            sb.AppendLine();
            sb.AppendLine("    /// <summary>Destructures the source tuple and applies <paramref name=\"transform\"/> to its elements.</summary>");
            sb.AppendLine($"    public static TResult I<{ty}, TResult>(this ({ty}) source, Func<{ty}, TResult> transform)");
            sb.AppendLine("    {");
            sb.AppendLine($"        return transform({tv});");
            sb.AppendLine("    }");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildStructuredConcurrency()
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace PipeEx.StructuredConcurrency;");
        sb.AppendLine();
        sb.AppendLine("public static class TupleDestructuring");
        sb.AppendLine("{");

        for (var arity = MinArity; arity <= MaxArity; arity++)
        {
            var ty = TypeParams(arity);
            var tv = Items(arity, "source");
            var targ = Items(arity, "t");

            sb.AppendLine();
            sb.AppendLine($"    public static async StructuredTask<TResult> I<{ty}, TResult>(this ({ty}) source, Func<{ty}, Task<TResult>> func)");
            sb.AppendLine("    {");
            sb.AppendLine($"        return await func({tv}).ConfigureAwait(false);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public static StructuredTask<TResult> I<{ty}, TResult>(this ({ty}) source, Func<{ty}, StructuredTask<TResult>> func)");
            sb.AppendLine("    {");
            sb.AppendLine("        // source is a value, so func runs eagerly with nothing to await first; return its handle");
            sb.AppendLine("        // directly, matching the single-source value -> StructuredTask overload.");
            sb.AppendLine($"        return func({tv});");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public static async StructuredTask<TResult> I<{ty}, TResult>(this Task<({ty})> s, Func<{ty}, TResult> func)");
            sb.AppendLine("    {");
            sb.AppendLine("        var source = await s.ConfigureAwait(false);");
            sb.AppendLine($"        return func({tv});");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public static StructuredTask<TResult> I<{ty}, TResult>(this StructuredTask<({ty})> s, Func<{ty}, TResult> func)");
            sb.AppendLine("    {");
            sb.AppendLine("        var impl = async () =>");
            sb.AppendLine("        {");
            sb.AppendLine("            var source = await s.ConfigureAwait(false);");
            sb.AppendLine($"            return func({tv});");
            sb.AppendLine("        };");
            sb.AppendLine();
            sb.AppendLine("        return new StructuredTask<TResult>(impl(), s);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public static async StructuredTask<TResult> I<{ty}, TResult>(this Task<({ty})> s, Func<{ty}, Task<TResult>> func)");
            sb.AppendLine("    {");
            sb.AppendLine("        var source = await s.ConfigureAwait(false);");
            sb.AppendLine($"        return await func({tv}).ConfigureAwait(false);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public static StructuredTask<TResult> I<{ty}, TResult>(this StructuredTask<({ty})> s, Func<{ty}, Task<TResult>> func)");
            sb.AppendLine("    {");
            sb.AppendLine("        var impl = async () =>");
            sb.AppendLine("        {");
            sb.AppendLine("            var source = await s.ConfigureAwait(false);");
            sb.AppendLine($"            return await func({tv}).ConfigureAwait(false);");
            sb.AppendLine("        };");
            sb.AppendLine();
            sb.AppendLine("        return new StructuredTask<TResult>(impl(), s);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public static StructuredTask<TResult> I<{ty}, TResult>(this Task<({ty})> s, Func<{ty}, StructuredTask<TResult>> func)");
            sb.AppendLine($"        => StructuredConcurrency.ChainTupleToStructured(s, t => func({targ}), new CancellationTokenSource());");
            sb.AppendLine();
            sb.AppendLine($"    public static StructuredTask<TResult> I<{ty}, TResult>(this StructuredTask<({ty})> s, Func<{ty}, StructuredTask<TResult>> func)");
            sb.AppendLine($"        => StructuredConcurrency.ChainTupleToStructured(s.Task, t => func({targ}), CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token));");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    // "TSource1, TSource2, ..., TSource{arity}"
    private static string TypeParams(int arity)
    {
        var sb = new StringBuilder("TSource1");
        for (var i = 2; i <= arity; i++)
            sb.Append($", TSource{i}");
        return sb.ToString();
    }

    // "{prefix}.Item1, {prefix}.Item2, ..., {prefix}.Item{arity}"
    private static string Items(int arity, string prefix)
    {
        var sb = new StringBuilder($"{prefix}.Item1");
        for (var i = 2; i <= arity; i++)
            sb.Append($", {prefix}.Item{i}");
        return sb.ToString();
    }
}
