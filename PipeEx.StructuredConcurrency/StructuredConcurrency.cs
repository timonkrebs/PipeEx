using System.Runtime.CompilerServices;

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
}
