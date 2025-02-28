namespace PipeEx;

public static class TupleDestructuring
{
    public static TResult I<TSource1, TSource2, TResult>(this (TSource1, TSource2) source, Func<TSource1, TSource2, TResult> func)
    {
        return func(source.Item1, source.Item2);
    }

    public static async Task<TResult> I<TSource1, TSource2, TResult>(this (TSource1, TSource2) source, Func<TSource1, TSource2, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2);
    }

    public static async Task<TResult> I<TSource1, TSource2, TResult>(this Task<(TSource1, TSource2)> s, Func<TSource1, TSource2, TResult> func)
    {
        var source = await s;
        return func(source.Item1, source.Item2);
    }

    public static async Task<TResult> I<TSource1, TSource2, TResult>(this Task<(TSource1, TSource2)> s, Func<TSource1, TSource2, Task<TResult>> func)
    {
        var source = await s;
        return await func(source.Item1, source.Item2);
    }
    public static TResult I<TSource1, TSource2, TSource3, TResult>(this (TSource1, TSource2, TSource3) source, Func<TSource1, TSource2, TSource3, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TResult>(this (TSource1, TSource2, TSource3) source, Func<TSource1, TSource2, TSource3, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TResult>(this Task<(TSource1, TSource2, TSource3)> s, Func<TSource1, TSource2, TSource3, TResult> func)
    {
        var source = await s;
        return func(source.Item1, source.Item2, source.Item3);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TResult>(this Task<(TSource1, TSource2, TSource3)> s, Func<TSource1, TSource2, TSource3, Task<TResult>> func)
    {
        var source = await s;
        return await func(source.Item1, source.Item2, source.Item3);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TResult>(this (TSource1, TSource2, TSource3, TSource4) source, Func<TSource1, TSource2, TSource3, TSource4, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this (TSource1, TSource2, TSource3, TSource4) source, Func<TSource1, TSource2, TSource3, TSource4, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4)> s, Func<TSource1, TSource2, TSource3, TSource4, TResult> func)
    {
        var source = await s;
        return func(source.Item1, source.Item2, source.Item3, source.Item4);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4)> s, Func<TSource1, TSource2, TSource3, TSource4, Task<TResult>> func)
    {
        var source = await s;
        return await func(source.Item1, source.Item2, source.Item3, source.Item4);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult> func)
    {
        var source = await s;
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, Task<TResult>> func)
    {
        var source = await s;
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult> func)
    {
        var source = await s;
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, Task<TResult>> func)
    {
        var source = await s;
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult> func)
    {
        var source = await s;
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, Task<TResult>> func)
    {
        var source = await s;
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult> func)
    {
        var source = await s;
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, Task<TResult>> func)
    {
        var source = await s;
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult> func)
    {
        var source = await s;
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, Task<TResult>> func)
    {
        var source = await s;
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult> func)
    {
        var source = await s;
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, Task<TResult>> func)
    {
        var source = await s;
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult> func)
    {
        var source = await s;
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, Task<TResult>> func)
    {
        var source = await s;
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult> func)
    {
        var source = await s;
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, Task<TResult>> func)
    {
        var source = await s;
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12);
    }
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult> func)
    {
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult> func)
    {
        var source = await s;
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13);
    }

    public static async Task<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, Task<TResult>> func)
    {
        var source = await s;
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13);
    }
}
