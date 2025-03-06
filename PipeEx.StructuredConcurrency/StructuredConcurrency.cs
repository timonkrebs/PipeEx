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
        var impl = async () => {
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
        var impl = async () => func(await source);
        return new StructuredTask<TResult>(impl(), source.CancellationTokenSource);
    }

    public static async StructuredTask<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, Task<TResult>> func)
    {
        return await func(await source);
    }

    public static StructuredTask<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, StructuredTask<TResult>> func)
    {
        // ToDo: Func that return StructuredTask must be handeled everywhere
        StructuredTask<TResult> structuredTask = default!;
        // ToDo: This works does not work because the structuredTask is not assigned befor the await is hit.
        var impl = async () => {
            structuredTask = func(await source);
            return await structuredTask;
        };

        return new StructuredTask<TResult>(impl(), structuredTask.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, Task<TResult>> func)
    {
        var impl = async () => await func(await source);
        return new StructuredTask<TResult>(impl(), source.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, StructuredTask<TResult>> func)
    {
        // ToDo: Func that return StructuredTask must be handeled everywhere
        StructuredTask<TResult> structuredTask = default!;
        // This works does not work because the structuredTask is not assigned befor the await is hit.
        var impl = async () => 
            {
                try
                {
                    await source;
                }
                catch(TaskCanceledException)
                {
 
                    await structuredTask.CancellationTokenSource.CancelAsync();
                    throw;
                }

                try
                {
                    structuredTask = func(source.Result);
                    structuredTask.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                        [structuredTask.CancellationTokenSource.Token, source.CancellationTokenSource.Token]);
                    return await structuredTask;
                }
                catch(TaskCanceledException)
                {
                    await source.CancellationTokenSource.CancelAsync();
                    throw;
                }
            };

        return new StructuredTask<TResult>(impl(), structuredTask.CancellationTokenSource);
    }
}
