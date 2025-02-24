﻿namespace PipeEx;

public static class TupleReturn
{
    public static (TResult, TResult2) I<TSource, TResult, TResult2>(this TSource source, Func<TSource, TResult> func, Func<TSource, TResult2> func2)
    {
        return (func(source), func2(source));
    }

    public static async Task<(TResult, TResult2)> I<TSource, TResult, TResult2>(this Task<TSource> source, Func<TSource, TResult> func, Func<TSource, TResult2> func2)
    {
        var s = await source;
        return (func(s), func2(s));
    }

    public static async Task<(TResult, TResult2)> I<TSource, TResult, TResult2>(this TSource source, Func<TSource, TResult> func, Func<TSource, Task<TResult2>> func2)
    {
        var t1 = func(source);
        var t2 = func2(source);
        return (t1, await t2);
    }

    public static async Task<(TResult, TResult2)> I<TSource, TResult, TResult2>(this TSource source, Func<TSource, Task<TResult>> func, Func<TSource, TResult2> func2)
    {
        var t1 = func(source);
        var t2 = func2(source);
        return (await t1, t2);
    }

    public static async Task<(TResult, TResult2)> I<TSource, TResult, TResult2>(this TSource source, Func<TSource, Task<TResult>> func, Func<TSource, Task<TResult2>> func2)
    {
        var t1 = func(source);
        var t2 = func2(source);
        await Task.WhenAll(t1, t2).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        return (t1.Result, t2.Result);
    }

    public static async Task<(TResult, TResult2)> I<TSource, TResult, TResult2>(this Task<TSource> source, Func<TSource, TResult> func, Func<TSource, Task<TResult2>> func2)
    {
        var s = await source;
        var t1 = func(s);
        var t2 = func2(s);
        return (t1, await t2);
    }

    public static async Task<(TResult, TResult2)> I<TSource, TResult, TResult2>(this Task<TSource> source, Func<TSource, Task<TResult>> func, Func<TSource, TResult2> func2)
    {
        var s = await source;
        var t1 = func(s);
        var t2 = func2(s);
        return (await t1, t2);
    }

    public static async Task<(TResult, TResult2)> I<TSource, TResult, TResult2>(this Task<TSource> source, Func<TSource, Task<TResult>> func, Func<TSource, Task<TResult2>> func2)
    {
        var s = await source;
        var t1 = func(s);
        var t2 = func2(s);
        await Task.WhenAll(t1, t2).ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        return (t1.Result, t2.Result);
    }
}
