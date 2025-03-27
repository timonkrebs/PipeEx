using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace PipeEx.StructuredConcurrency;

public static class StructuredConcurrency
{
    [OverloadResolutionPriority(1)]
    public static async StructuredTask<TResult> I<TSource, TResult>(this TSource source, Func<TSource, Task<TResult>> func)
    {
        return await func(source);
    }

    public static StructuredTask<TResult> I<TSource, TResult>(this TSource source, Func<TSource, StructuredTask<TResult>> func)
    {
        // This works because the structuredTask is assigned befor the await is hit.
        StructuredTask<TResult> structuredTask = default!;
        var impl = async () =>
        {
            structuredTask = func(source);
            return await structuredTask;
        };

        impl();
        return structuredTask;
    }

    public static async StructuredTask<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, TResult> func)
    {
        return func(await source);
    }

    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, TResult> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var s = await source;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = func(s);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        };
        return new StructuredTask<TResult>(impl(), source.CancellationTokenSource);
    }

    public static async StructuredTask<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, Task<TResult>> func)
    {
        return await func(await source);
    }

    public static StructuredTask<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, StructuredTask<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                if (source.IsCanceled)
                {
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetException(new OperationCanceledException());
                    return;
                }
                var innerStructuredTask = func(await source);

                using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                var result = await innerStructuredTask;

                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                cts.Cancel(); // Ensure consistent cancellation.
                tcs.SetException(ex);
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, Task<TResult>> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var s = await source;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = await func(s);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        };
        return new StructuredTask<TResult>(impl(), source.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, StructuredTask<TResult>> func)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(source.CancellationTokenSource.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {

            try
            {
                if (source.Task.IsCanceled)
                {
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetException(new OperationCanceledException());
                    return;
                }

                var innerStructuredTask = func(await source);
                using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                var result = await innerStructuredTask;

                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                cts.Cancel(); // Ensure consistent cancellation.
                tcs.SetException(ex);
            }
        };
        impl();
        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredDeferedTask<TSource, TDeferd> Let<TSource, TDeferd>(this TSource source, Func<Task<TDeferd>> func) => 
        new StructuredDeferedTask<TSource, TDeferd>(Task.FromResult(source), func());

    public static StructuredDeferedTask<TSource, TDeferd> Let<TSource, TDeferd>(this TSource source, Func<TSource, Task<TDeferd>> func) => 
        new StructuredDeferedTask<TSource, TDeferd>(Task.FromResult(source), func(source));

    public static StructuredDeferedTask<TSource, TDeferd> Let<TSource, TDeferd>(this StructuredTask<TSource> source, Func<TSource, Task<TDeferd>> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var s = await source;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = await func(s);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        };

        return new StructuredDeferedTask<TSource, TDeferd>(source, impl(), source.CancellationTokenSource);
    }

    public static StructuredDeferedTask<TSource, TDeferd> Let<TSource, TDeferd>(this StructuredTask<TSource> source, Func<Task<TDeferd>> func) => 
        new StructuredDeferedTask<TSource, TDeferd>(source, func(), source.CancellationTokenSource);

    public static StructuredDeferedTask<TSource, TDeferd1, TDeferd2> Let<TSource, TDeferd1, TDeferd2>(this StructuredDeferedTask<TSource, TDeferd1> source, Func<TSource, Task<TDeferd2>> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var s = await source;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = await func(s);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        };

        return new StructuredDeferedTask<TSource, TDeferd1, TDeferd2>(source.Task, source.deferedTask1, impl(), source.CancellationTokenSource);
    }

    public static StructuredDeferedTask<TSource, TDeferd1, TDeferd2> Let<TSource, TDeferd1, TDeferd2>(this StructuredDeferedTask<TSource, TDeferd1> source, Func<Task<TDeferd2>> func) => 
        new StructuredDeferedTask<TSource, TDeferd1, TDeferd2>(source.Task, source.deferedTask1, func());


    public static StructuredDeferedTask<TSource, TDeferd> Let<TSource, TDeferd>(this TSource source, Func<TSource, StructuredTask<TDeferd>> func)
    {
        var innerStructuredTask = func(source);
        // Create a new CTS for the resulting StructuredDeferedTask
        var cts = new CancellationTokenSource();
        // Ensure the new CTS can cancel the inner task
        var registration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());

        var deferedCompletionSource = new TaskCompletionSource<TDeferd>();
        // Wrap the inner task's await logic to handle completion and cancellation propagation
        var wrapperTask = async () => {
            try {
                var result = await innerStructuredTask;
                deferedCompletionSource.SetResult(result);
            } catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token || ex.CancellationToken == innerStructuredTask.CancellationTokenSource.Token) {
                // If cancellation occurs either via the new CTS or the inner task's CTS, cancel the wrapper.
                deferedCompletionSource.SetCanceled(cts.Token);
            } catch (Exception ex) {
                deferedCompletionSource.SetException(ex);
                cts.Cancel(); // If the inner task fails, cancel the wrapper CTS.
            } finally {
                registration.Dispose(); // Clean up the cancellation registration
            }
        };
        wrapperTask(); // Fire and forget

        // Return a new StructuredDeferedTask using the appropriate constructor
        return new StructuredDeferedTask<TSource, TDeferd>(Task.FromResult(source), deferedCompletionSource.Task, cts);
    }

    public static StructuredDeferedTask<TSource, TDeferd> Let<TSource, TDeferd>(this StructuredTask<TSource> source, Func<TSource, StructuredTask<TDeferd>> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        // Link the new CTS to the source task's CTS
        var cts = CancellationTokenSource.CreateLinkedTokenSource(source.CancellationTokenSource.Token);
        var deferedTaskCompletionSource = new TaskCompletionSource<TDeferd>();
        StructuredTask<TDeferd>? innerStructuredTask = null; // Declare here for access in catch block

        var deferedImpl = async () =>
        {
            try
            {
                var s = await source.Task; // Await the source task first
                cts.Token.ThrowIfCancellationRequested(); // Check cancellation after source completes

                innerStructuredTask = func(s); // Execute the function to get the inner task
                // Link the combined token to the inner task
                using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());

                var result = await innerStructuredTask; // Await the inner task
                deferedTaskCompletionSource.SetResult(result);
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token || (innerStructuredTask != null && ex.CancellationToken == innerStructuredTask.CancellationTokenSource.Token))
            {
               // If cancellation comes from the linked token or the inner task itself
               deferedTaskCompletionSource.SetCanceled(cts.Token);
            }
            catch (Exception ex)
            {
                deferedTaskCompletionSource.SetException(ex);
                cts.Cancel(); // Cancel related tasks on exception
            }
        };
        deferedImpl(); // Fire and forget

        // Use the constructor: Task<T>, Task<TDeferd>, CancellationTokenSource
        return new StructuredDeferedTask<TSource, TDeferd>(source.Task, deferedTaskCompletionSource.Task, cts);
    }

    public static StructuredDeferedTask<TSource, TDeferd> Let<TSource, TSource2, TDeferd>(this TSource source, Func<TSource, StructuredDeferedTask<TSource2, TDeferd>> func)
    {
        var innerDeferedTask = func(source);
        // Create a new CTS for the resulting StructuredDeferedTask
        var cts = new CancellationTokenSource();
        // Ensure the new CTS can cancel the inner defered task
        var registration = cts.Token.Register(() => innerDeferedTask.CancellationTokenSource.Cancel());

        var deferedCompletionSource = new TaskCompletionSource<TDeferd>();
        // Wrap the inner task's await logic
        var wrapperTask = async () => {
            try {
                var result = await innerDeferedTask.deferedTask1; // Await the inner defered task
                deferedCompletionSource.SetResult(result);
            } catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token || ex.CancellationToken == innerDeferedTask.CancellationTokenSource.Token) {
                // If cancellation occurs either via the new CTS or the inner task's CTS, cancel the wrapper.
                deferedCompletionSource.SetCanceled(cts.Token);
            } catch (Exception ex) {
                deferedCompletionSource.SetException(ex);
                cts.Cancel(); // If the inner task fails, cancel the wrapper CTS.
            } finally {
                registration.Dispose(); // Clean up registration
            }
        };
        wrapperTask(); // Fire and forget

        // Return a new StructuredDeferedTask
        return new StructuredDeferedTask<TSource, TDeferd>(Task.FromResult(source), deferedCompletionSource.Task, cts);
    }

    public static StructuredDeferedTask<TSource, TSource2, TDeferd> Let<TSource, TSource2, TDeferd>(this StructuredTask<TSource> source, Func<TSource, StructuredDeferedTask<TSource2, TDeferd>> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        // Link the new CTS to the source task's CTS
        var cts = CancellationTokenSource.CreateLinkedTokenSource(source.CancellationTokenSource.Token);
        var deferedTaskCompletionSource = new TaskCompletionSource<TDeferd>();
        StructuredDeferedTask<TSource2, TDeferd> innerDeferedTask = null; // Declare here for access in catch block

        var deferedImpl = async () =>
        {
            try
            {
                var s = await source.Task; // Await the source task
                cts.Token.ThrowIfCancellationRequested(); // Check for cancellation

                innerDeferedTask = func(s); // Execute the function
                // Link the combined token to the inner defered task's source
                using var innerRegistration = cts.Token.Register(() => innerDeferedTask.CancellationTokenSource.Cancel());

                var result = await innerDeferedTask.deferedTask1; // Await the inner defered task
                deferedTaskCompletionSource.SetResult(result);
            }
             catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token || (innerDeferedTask != null && ex.CancellationToken == innerDeferedTask.CancellationTokenSource.Token))
            {
                // If cancellation comes from the linked token or the inner task itself
                deferedTaskCompletionSource.SetCanceled(cts.Token);
            }
            catch (Exception ex)
            {
                deferedTaskCompletionSource.SetException(ex);
                cts.Cancel(); // Cancel related tasks on exception
            }
        };
        deferedImpl(); // Fire and forget

        // Use the constructor: Task<T>, Task<TDeferd>, CancellationTokenSource
        return new StructuredDeferedTask<TSource, TSource2, TDeferd>(source.Task, innerDeferedTask.Task, deferedTaskCompletionSource.Task, cts);
    }

    public static StructuredDeferedTask<TResult, TDeferedSource> Await<TSource, TDeferedSource, TResult>(this StructuredDeferedTask<TSource, TDeferedSource> source, Func<TSource, TDeferedSource, TResult> func, [CallerArgumentExpression("func")] string propertyName = "")
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var s = await source.Task;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var d = await source.deferedTask1;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = func(s, d);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        };

        var sdt = new StructuredDeferedTask<TResult, TDeferedSource>(impl(), source.deferedTask1, source.CancellationTokenSource);
        return sdt;
    }

    public static StructuredDeferedTask<TResult, TDeferedSource1, TDeferedSource2> Await<TSource, TDeferedSource1, TDeferedSource2, TResult>(this StructuredDeferedTask<TSource, TDeferedSource1, TDeferedSource2> source, Func<TSource, TDeferedSource1, TDeferedSource2, TResult> func, [CallerArgumentExpression("func")] string propertyName = "")
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            Regex regex = new Regex(@"\(([^)]*)\)");
            Match match = regex.Match(propertyName);
            var discard = Regex.Split(match.Groups[1].Value, @",\s*").Select(x => x.Trim().StartsWith("_")).ToArray();

            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var s = discard[0] ? default! : await source.Task;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var d1 = discard[1] ? default! : await source.deferedTask1;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var d2 = discard[2] ? default! : await source.deferedTask2;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = func(s, d1, d2);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        };

        var sdt = new StructuredDeferedTask<TResult, TDeferedSource1, TDeferedSource2>(impl(), source.deferedTask1, source.deferedTask2, source.CancellationTokenSource);
        return sdt;
    }
}
