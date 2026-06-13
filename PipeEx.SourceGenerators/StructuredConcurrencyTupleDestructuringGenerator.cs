using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace PipeEx.SourceGenerators;

/// <summary>
/// Generates the <c>PipeEx.StructuredConcurrency.TupleDestructuring</c> class. For every
/// tuple arity from <see cref="TupleArity.Min"/> to <see cref="TupleArity.Max"/> it emits
/// the full set of <c>I</c> overloads that bridge tuple sources (plain, <c>Task</c> and
/// <c>StructuredTask</c>) with synchronous, <c>Task</c> and <c>StructuredTask</c> transforms.
/// </summary>
/// <remarks>
/// Replaces the former <c>PipeEx.StructuredConcurrency/StructuredConcurrency.TupleDestructuring.sh</c> shell script.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class StructuredConcurrencyTupleDestructuringGenerator : IIncrementalGenerator
{
    /// <summary>The generators are shipped in one shared assembly, so each one only emits into its target assembly.</summary>
    private const string TargetAssembly = "PipeEx.StructuredConcurrency";

    private const string HintName = "StructuredConcurrency.TupleDestructuring.g.cs";

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
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine();
        sb.AppendLine("namespace PipeEx.StructuredConcurrency;");
        sb.AppendLine();
        sb.AppendLine("public static class TupleDestructuring");
        sb.AppendLine("{");

        for (var arity = TupleArity.Min; arity <= TupleArity.Max; arity++)
        {
            sb.Append(OverloadsTemplate
                .Replace("$ty$", TupleArity.TypeList(arity))
                .Replace("$tv$", TupleArity.ValueList(arity)));
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    // The full set of overloads emitted for a single tuple arity. The leading and trailing
    // blank lines keep one blank line between consecutive arities and before the closing
    // brace of the class. The closing delimiter sits in column 0 so the template is copied
    // verbatim (no raw-string indentation is stripped).
    private const string OverloadsTemplate =
"""

    public static async StructuredTask<TResult> I<$ty$, TResult>(this ($ty$) source, Func<$ty$, Task<TResult>> func)
    {
        return await func($tv$);
    }

    public static StructuredTask<TResult> I<$ty$, TResult>(this ($ty$) source, Func<$ty$, StructuredTask<TResult>> func)
    {
        // This works because the structuredTask is assigned before the await is hit.
        StructuredTask<TResult> structuredTask = default!;
        var impl = async () =>
        {
            structuredTask = func($tv$);
            return await structuredTask;
        };

        return new StructuredTask<TResult>(impl(), structuredTask);
    }

    public static async StructuredTask<TResult> I<$ty$, TResult>(this Task<($ty$)> s, Func<$ty$, TResult> func)
    {
        var source = await s;
        return func($tv$);
    }

    public static StructuredTask<TResult> I<$ty$, TResult>(this StructuredTask<($ty$)> s, Func<$ty$, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func($tv$);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static async StructuredTask<TResult> I<$ty$, TResult>(this Task<($ty$)> s, Func<$ty$, Task<TResult>> func)
    {
        var source = await s;
        return await func($tv$);
    }

    public static StructuredTask<TResult> I<$ty$, TResult>(this StructuredTask<($ty$)> s, Func<$ty$, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func($tv$);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static StructuredTask<TResult> I<$ty$, TResult>(this Task<($ty$)> s, Func<$ty$, StructuredTask<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                ($ty$) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func($tv$);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
                    tcs.SetResult(result);
                }
                catch (OperationCanceledException)
                {
                    tcs.SetCanceled(innerStructuredTask.CancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }
            catch (Exception ex)
            {
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<$ty$, TResult>(this StructuredTask<($ty$)> s, Func<$ty$, StructuredTask<TResult>> func)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                ($ty$) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func($tv$);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
                    tcs.SetResult(result);
                }
                catch (OperationCanceledException)
                {
                    tcs.SetCanceled(innerStructuredTask.CancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }
            catch (Exception ex)
            {
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

""";
}
