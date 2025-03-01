namespace PipeEx.StructuredConcurrency;

public static class Branching
{
    public static async StructuredTask<(TResult, TResult2)> I<TSource, TResult, TResult2>(this TSource source, Func<TSource, Task<TResult>> func, Func<TSource, TResult2> func2)
    {
        var t1 = func(source);
        var t2 = func2(source);
        return (await t1, t2);
    }

    public static async StructuredTask<(TResult, TResult2)> I<TSource, TResult, TResult2>(this TSource source, Func<TSource, Task<TResult>> func, Func<TSource, Task<TResult2>> func2)
    {
        var t1 = func(source);
        var t2 = func2(source);
        await Task.WhenAll(t1, t2).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        return (t1.Result, t2.Result);
    }

    public static async StructuredTask<(TResult, TResult2)> I<TSource, TResult, TResult2>(this Task<TSource> source, Func<TSource, Task<TResult>> func, Func<TSource, TResult2> func2)
    {
        var s = await source;
        var t1 = func(s);
        var t2 = func2(s);
        return (await t1, t2);
    }

    public static StructuredTask<(TResult, TResult2)> I<TSource, TResult, TResult2>(this StructuredTask<TSource> source, Func<TSource, Task<TResult>> func, Func<TSource, TResult2> func2)
    {
        var impl = async (StructuredTask<TSource> source, Func<TSource, Task<TResult>> func, Func<TSource, TResult2> func2) =>
            {
                var s = await source;
                var t1 = func(s);
                var t2 = func2(s);
                return (await t1, t2);
            };

        return new StructuredTask<(TResult, TResult2)>(impl(source, func, func2), source.CancellationTokenSource);
    }

    public static async StructuredTask<(TResult, TResult2)> I<TSource, TResult, TResult2>(this Task<TSource> source, Func<TSource, Task<TResult>> func, Func<TSource, Task<TResult2>> func2)
    {
        var s = await source;
        var t1 = func(s);
        var t2 = func2(s);
        await Task.WhenAll(t1, t2).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        return (t1.Result, t2.Result);
    }

    public static StructuredTask<(TResult, TResult2)> I<TSource, TResult, TResult2>(this StructuredTask<TSource> source, Func<TSource, Task<TResult>> func, Func<TSource, Task<TResult2>> func2)
    {
        var impl = async (StructuredTask<TSource> source, Func<TSource, Task<TResult>> func, Func<TSource, Task<TResult2>> func2) =>
            {
                var s = await source;
                var t1 = func(s);
                var t2 = func2(s);
                await Task.WhenAll(t1, t2).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
                return (t1.Result, t2.Result);
            };

        return new StructuredTask<(TResult, TResult2)>(impl(source, func, func2), source.CancellationTokenSource);
    }

    public static async StructuredTask<(TResult, TResult2)> I<TSource, TSource2, TResult, TResult2>(this (TSource, TSource2) source, Func<TSource, TSource2, Task<TResult>> func, Func<TSource, TSource2, TResult2> func2)
    {
        var t1 = func(source.Item1, source.Item2);
        var t2 = func2(source.Item1, source.Item2);
        return (await t1, t2);
    }

    public static async StructuredTask<(TResult, TResult2)> I<TSource, TSource2, TResult, TResult2>(this (TSource, TSource2) source, Func<TSource, TSource2, Task<TResult>> func, Func<TSource, TSource2, Task<TResult2>> func2)
    {
        var t1 = func(source.Item1, source.Item2);
        var t2 = func2(source.Item1, source.Item2);
        await Task.WhenAll(t1, t2).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        return (t1.Result, t2.Result);
    }

    public static async StructuredTask<(TResult, TResult2)> I<TSource, TSource2, TResult, TResult2>(this Task<(TSource, TSource2)> source, Func<TSource, TSource2, Task<TResult>> func, Func<TSource, TSource2, TResult2> func2)
    {
        var s = await source;
        var t1 = func(s.Item1, s.Item2);
        var t2 = func2(s.Item1, s.Item2);
        return (await t1, t2);
    }

    public static StructuredTask<(TResult, TResult2)> I<TSource, TSource2, TResult, TResult2>(this StructuredTask<(TSource, TSource2)> source, Func<TSource, TSource2, Task<TResult>> func, Func<TSource, TSource2, TResult2> func2)
    {
        var impl = async (StructuredTask<(TSource, TSource2)> source, Func<TSource, TSource2, Task<TResult>> func, Func<TSource, TSource2, TResult2> func2) =>
            {
                var s = await source;
                var t1 = func(s.Item1, s.Item2);
                var t2 = func2(s.Item1, s.Item2);
                return (await t1, t2);
            };

        return new StructuredTask<(TResult, TResult2)>(impl(source, func, func2), source.CancellationTokenSource);
    }

    public static async StructuredTask<(TResult, TResult2)> I<TSource, TSource2, TResult, TResult2>(this Task<(TSource, TSource2)> source, Func<TSource, TSource2, Task<TResult>> func, Func<TSource, TSource2, Task<TResult2>> func2)
    {
        var s = await source;
        var t1 = func(s.Item1, s.Item2);
        var t2 = func2(s.Item1, s.Item2);
        await Task.WhenAll(t1, t2).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        return (t1.Result, t2.Result);
    }

    public static StructuredTask<(TResult, TResult2)> I<TSource, TSource2, TResult, TResult2>(this StructuredTask<(TSource, TSource2)> source, Func<TSource, TSource2, Task<TResult>> func, Func<TSource, TSource2, Task<TResult2>> func2)
    {
        var impl = async (StructuredTask<(TSource, TSource2)> source, Func<TSource, TSource2, Task<TResult>> func, Func<TSource, TSource2, Task<TResult2>> func2) =>
            {
                var s = await source;
                var t1 = func(s.Item1, s.Item2);
                var t2 = func2(s.Item1, s.Item2);
                await Task.WhenAll(t1, t2).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
                return (t1.Result, t2.Result);
            };

        return new StructuredTask<(TResult, TResult2)>(impl(source, func, func2), source.CancellationTokenSource);
    }
}
