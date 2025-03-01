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

    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, StructuredTask<TResult>> func)
    {
        // ToDo: This is problematic because i only get the structuredTask after i awaited the source
        // That is to late for new StructuredTask<TResult>(impl(), structuredTask!.CancellationTokenSource)
        StructuredTask<TResult> structuredTask = default!;
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
}