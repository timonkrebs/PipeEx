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
/// <item><c>PipeEx.StructuredConcurrency</c> gets the eleven StructuredTask <c>I</c> overloads per arity
/// (eight token-free and three cancellation-aware).</item>
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
            sb.AppendLine("        // Mirrors StructuredConcurrency.CheckedChain (keep in sync): up-front synchronous check,");
            sb.AppendLine("        // source await gated by CheckedAwait, trailing check after the projection — so the tuple");
            sb.AppendLine("        // path observes cancellation exactly like the scalar StructuredTask-source overload.");
            sb.AppendLine("        var ct = s.CancellationTokenSource.Token;");
            sb.AppendLine("        ct.ThrowIfCancellationRequested();");
            sb.AppendLine("        var impl = async () =>");
            sb.AppendLine("        {");
            sb.AppendLine("            var source = await s.Task.CheckedAwait(ct).ConfigureAwait(false);");
            sb.AppendLine($"            var result = func({tv});");
            sb.AppendLine("            ct.ThrowIfCancellationRequested();");
            sb.AppendLine("            return result;");
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
            sb.AppendLine("        // Mirrors StructuredConcurrency.CheckedChain (keep in sync): up-front synchronous check and");
            sb.AppendLine("        // every await gated by CheckedAwait — so the tuple path observes cancellation exactly like");
            sb.AppendLine("        // the scalar StructuredTask-source overload.");
            sb.AppendLine("        var ct = s.CancellationTokenSource.Token;");
            sb.AppendLine("        ct.ThrowIfCancellationRequested();");
            sb.AppendLine("        var impl = async () =>");
            sb.AppendLine("        {");
            sb.AppendLine("            var source = await s.Task.CheckedAwait(ct).ConfigureAwait(false);");
            sb.AppendLine($"            return await func({tv}).CheckedAwait(ct).ConfigureAwait(false);");
            sb.AppendLine("        };");
            sb.AppendLine();
            sb.AppendLine("        return new StructuredTask<TResult>(impl(), s);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public static StructuredTask<TResult> I<{ty}, TResult>(this Task<({ty})> s, Func<{ty}, StructuredTask<TResult>> func)");
            sb.AppendLine($"        => StructuredConcurrency.ChainTupleToStructured(s, t => func({targ}), new CancellationTokenSource());");
            sb.AppendLine();
            sb.AppendLine($"    public static StructuredTask<TResult> I<{ty}, TResult>(this StructuredTask<({ty})> s, Func<{ty}, StructuredTask<TResult>> func)");
            sb.AppendLine("        // Shares the source's CancellationTokenSource (ownership transfer) so cancelling the returned");
            sb.AppendLine("        // pipe reaches upstream stages — a linked child source would only propagate downstream.");
            sb.AppendLine($"        => StructuredConcurrency.ChainStructuredToStructured(s, t => func({targ}));");

            // Cancellation-aware tuple overloads: destructure the source and hand the carried token to the
            // job (the value/Task sources own a fresh CancellationTokenSource; the StructuredTask source
            // shares its own) so a running operation can be cancelled in flight, not only between stages.
            // Every await is gated by CheckedAwait — the same checked path the scalar token overloads use —
            // so cancellation is honoured between the source and the job, and after the job, even when the
            // job ignores the token it was handed.
            sb.AppendLine();
            sb.AppendLine($"    public static StructuredTask<TResult> I<{ty}, TResult>(this ({ty}) source, Func<{ty}, CancellationToken, Task<TResult>> func)");
            sb.AppendLine("    {");
            sb.AppendLine("        var cts = new CancellationTokenSource();");
            sb.AppendLine($"        var impl = async () => await func({tv}, cts.Token).CheckedAwait(cts.Token).ConfigureAwait(false);");
            sb.AppendLine("        return new StructuredTask<TResult>(impl(), cts);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public static StructuredTask<TResult> I<{ty}, TResult>(this Task<({ty})> s, Func<{ty}, CancellationToken, Task<TResult>> func)");
            sb.AppendLine("    {");
            sb.AppendLine("        var cts = new CancellationTokenSource();");
            sb.AppendLine("        var impl = async () =>");
            sb.AppendLine("        {");
            sb.AppendLine("            var source = await s.CheckedAwait(cts.Token).ConfigureAwait(false);");
            sb.AppendLine($"            return await func({tv}, cts.Token).CheckedAwait(cts.Token).ConfigureAwait(false);");
            sb.AppendLine("        };");
            sb.AppendLine("        return new StructuredTask<TResult>(impl(), cts);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public static StructuredTask<TResult> I<{ty}, TResult>(this StructuredTask<({ty})> s, Func<{ty}, CancellationToken, Task<TResult>> func)");
            sb.AppendLine("    {");
            sb.AppendLine("        // Inlines the token-aware StructuredConcurrency.CheckedChain pattern (keep in sync): the");
            sb.AppendLine("        // tuple func signature cannot be fed to CheckedChain without an adapter, so the up-front");
            sb.AppendLine("        // check + CheckedAwait-gated awaits are reproduced here verbatim.");
            sb.AppendLine("        var ct = s.CancellationTokenSource.Token;");
            sb.AppendLine("        ct.ThrowIfCancellationRequested();");
            sb.AppendLine("        var impl = async () =>");
            sb.AppendLine("        {");
            sb.AppendLine("            var source = await s.Task.CheckedAwait(ct).ConfigureAwait(false);");
            sb.AppendLine($"            return await func({tv}, ct).CheckedAwait(ct).ConfigureAwait(false);");
            sb.AppendLine("        };");
            sb.AppendLine("        return new StructuredTask<TResult>(impl(), s);");
            sb.AppendLine("    }");
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
