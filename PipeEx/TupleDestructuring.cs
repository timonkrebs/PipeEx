namespace PipeEx;

public static class TupleDestructuring
{
    public static TResult I<TSource, TSource2, TResult>(this (TSource, TSource2) source, Func<TSource, TSource2, TResult> func)
    {
        return func(source.Item1, source.Item2);
    }

    public static async Task<TResult> I<TSource, TSource2, TResult>(this (TSource, TSource2) source, Func<TSource, TSource2, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2);
    }

    public static async Task<TResult> I<TSource, TSource2, TResult>(this Task<(TSource, TSource2)> source, Func<TSource, TSource2, TResult> func)
    {
        var s = await source;
        return func(s.Item1, s.Item2);
    }

    public static async Task<TResult> I<TSource, TSource2, TResult>(this Task<(TSource, TSource2)> source, Func<TSource, TSource2, Task<TResult>> func)
    {
        var s = await source;
        return await func(s.Item1, s.Item2);
    }
}
