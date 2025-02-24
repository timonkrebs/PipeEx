﻿namespace PipeEx;

public static class Core
{
    public static TResult I<TSource, TResult>(this TSource source, Func<TSource, TResult> func)
    {
        return func(source);
    }

    public static async Task<TResult> I<TSource, TResult>(this TSource source, Func<TSource, Task<TResult>> func)
    {
        return await func(source);
    }

    public static async Task<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, TResult> func)
    {
        return func(await source);
    }

    public static async Task<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, Task<TResult>> func)
    {
        return await func(await source);
    }
}
