namespace PipeEx.StructuredConcurrency;

public static class StructuredConcurrency
{
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

        return new StructuredTask<TResult>(impl(), structuredTask.CancellationTokenSource);
    }

    public static async StructuredTask<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, TResult> func)
    {
        return func(await source);
    }

    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, TResult> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () => {
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
                TSource sourceResult;
                try
                {
                    sourceResult = await source;
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

                var innerStructuredTask = func(sourceResult);

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

    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, Task<TResult>> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () => {
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
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(source.CancellationTokenSource.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                TSource sourceResult;
                try
                {
                    sourceResult = await source;
                    source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
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

                var innerStructuredTask = func(sourceResult);
                source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
                    source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
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
}
