namespace PipeEx.StructuredConcurrency;

public static class TupleDestructuring
{

    public static async StructuredTask<TResult> I<TSource1, TSource2, TResult>(this (TSource1, TSource2) source, Func<TSource1, TSource2, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TResult>(this (TSource1, TSource2) source, Func<TSource1, TSource2, StructuredTask<TResult>> func)
    {
        // source is a value, so func runs eagerly with nothing to await first; return its handle
        // directly, matching the single-source value -> StructuredTask overload.
        return func(source.Item1, source.Item2);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TResult>(this Task<(TSource1, TSource2)> s, Func<TSource1, TSource2, TResult> func)
    {
        var source = await s.ConfigureAwait(false);
        return func(source.Item1, source.Item2);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TResult>(this StructuredTask<(TSource1, TSource2)> s, Func<TSource1, TSource2, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return func(source.Item1, source.Item2);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TResult>(this Task<(TSource1, TSource2)> s, Func<TSource1, TSource2, Task<TResult>> func)
    {
        var source = await s.ConfigureAwait(false);
        return await func(source.Item1, source.Item2).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TResult>(this StructuredTask<(TSource1, TSource2)> s, Func<TSource1, TSource2, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return await func(source.Item1, source.Item2).ConfigureAwait(false);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TResult>(this Task<(TSource1, TSource2)> s, Func<TSource1, TSource2, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s, t => func(t.Item1, t.Item2), new CancellationTokenSource());

    public static StructuredTask<TResult> I<TSource1, TSource2, TResult>(this StructuredTask<(TSource1, TSource2)> s, Func<TSource1, TSource2, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s.Task, t => func(t.Item1, t.Item2), CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token));

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TResult>(this (TSource1, TSource2, TSource3) source, Func<TSource1, TSource2, TSource3, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TResult>(this (TSource1, TSource2, TSource3) source, Func<TSource1, TSource2, TSource3, StructuredTask<TResult>> func)
    {
        // source is a value, so func runs eagerly with nothing to await first; return its handle
        // directly, matching the single-source value -> StructuredTask overload.
        return func(source.Item1, source.Item2, source.Item3);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TResult>(this Task<(TSource1, TSource2, TSource3)> s, Func<TSource1, TSource2, TSource3, TResult> func)
    {
        var source = await s.ConfigureAwait(false);
        return func(source.Item1, source.Item2, source.Item3);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TResult>(this StructuredTask<(TSource1, TSource2, TSource3)> s, Func<TSource1, TSource2, TSource3, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return func(source.Item1, source.Item2, source.Item3);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TResult>(this Task<(TSource1, TSource2, TSource3)> s, Func<TSource1, TSource2, TSource3, Task<TResult>> func)
    {
        var source = await s.ConfigureAwait(false);
        return await func(source.Item1, source.Item2, source.Item3).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TResult>(this StructuredTask<(TSource1, TSource2, TSource3)> s, Func<TSource1, TSource2, TSource3, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return await func(source.Item1, source.Item2, source.Item3).ConfigureAwait(false);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TResult>(this Task<(TSource1, TSource2, TSource3)> s, Func<TSource1, TSource2, TSource3, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s, t => func(t.Item1, t.Item2, t.Item3), new CancellationTokenSource());

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TResult>(this StructuredTask<(TSource1, TSource2, TSource3)> s, Func<TSource1, TSource2, TSource3, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s.Task, t => func(t.Item1, t.Item2, t.Item3), CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token));

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this (TSource1, TSource2, TSource3, TSource4) source, Func<TSource1, TSource2, TSource3, TSource4, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this (TSource1, TSource2, TSource3, TSource4) source, Func<TSource1, TSource2, TSource3, TSource4, StructuredTask<TResult>> func)
    {
        // source is a value, so func runs eagerly with nothing to await first; return its handle
        // directly, matching the single-source value -> StructuredTask overload.
        return func(source.Item1, source.Item2, source.Item3, source.Item4);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4)> s, Func<TSource1, TSource2, TSource3, TSource4, TResult> func)
    {
        var source = await s.ConfigureAwait(false);
        return func(source.Item1, source.Item2, source.Item3, source.Item4);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4)> s, Func<TSource1, TSource2, TSource3, TSource4, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return func(source.Item1, source.Item2, source.Item3, source.Item4);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4)> s, Func<TSource1, TSource2, TSource3, TSource4, Task<TResult>> func)
    {
        var source = await s.ConfigureAwait(false);
        return await func(source.Item1, source.Item2, source.Item3, source.Item4).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4)> s, Func<TSource1, TSource2, TSource3, TSource4, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return await func(source.Item1, source.Item2, source.Item3, source.Item4).ConfigureAwait(false);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4)> s, Func<TSource1, TSource2, TSource3, TSource4, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s, t => func(t.Item1, t.Item2, t.Item3, t.Item4), new CancellationTokenSource());

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4)> s, Func<TSource1, TSource2, TSource3, TSource4, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s.Task, t => func(t.Item1, t.Item2, t.Item3, t.Item4), CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token));

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, StructuredTask<TResult>> func)
    {
        // source is a value, so func runs eagerly with nothing to await first; return its handle
        // directly, matching the single-source value -> StructuredTask overload.
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult> func)
    {
        var source = await s.ConfigureAwait(false);
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, Task<TResult>> func)
    {
        var source = await s.ConfigureAwait(false);
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5).ConfigureAwait(false);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5), new CancellationTokenSource());

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s.Task, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5), CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token));

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, StructuredTask<TResult>> func)
    {
        // source is a value, so func runs eagerly with nothing to await first; return its handle
        // directly, matching the single-source value -> StructuredTask overload.
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult> func)
    {
        var source = await s.ConfigureAwait(false);
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, Task<TResult>> func)
    {
        var source = await s.ConfigureAwait(false);
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6).ConfigureAwait(false);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6), new CancellationTokenSource());

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s.Task, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6), CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token));

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, StructuredTask<TResult>> func)
    {
        // source is a value, so func runs eagerly with nothing to await first; return its handle
        // directly, matching the single-source value -> StructuredTask overload.
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult> func)
    {
        var source = await s.ConfigureAwait(false);
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, Task<TResult>> func)
    {
        var source = await s.ConfigureAwait(false);
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7).ConfigureAwait(false);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7), new CancellationTokenSource());

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s.Task, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7), CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token));

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, StructuredTask<TResult>> func)
    {
        // source is a value, so func runs eagerly with nothing to await first; return its handle
        // directly, matching the single-source value -> StructuredTask overload.
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult> func)
    {
        var source = await s.ConfigureAwait(false);
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, Task<TResult>> func)
    {
        var source = await s.ConfigureAwait(false);
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8).ConfigureAwait(false);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8), new CancellationTokenSource());

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s.Task, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8), CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token));

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, StructuredTask<TResult>> func)
    {
        // source is a value, so func runs eagerly with nothing to await first; return its handle
        // directly, matching the single-source value -> StructuredTask overload.
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult> func)
    {
        var source = await s.ConfigureAwait(false);
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, Task<TResult>> func)
    {
        var source = await s.ConfigureAwait(false);
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9).ConfigureAwait(false);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, t.Item9), new CancellationTokenSource());

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s.Task, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, t.Item9), CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token));

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, StructuredTask<TResult>> func)
    {
        // source is a value, so func runs eagerly with nothing to await first; return its handle
        // directly, matching the single-source value -> StructuredTask overload.
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult> func)
    {
        var source = await s.ConfigureAwait(false);
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, Task<TResult>> func)
    {
        var source = await s.ConfigureAwait(false);
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10).ConfigureAwait(false);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, t.Item9, t.Item10), new CancellationTokenSource());

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s.Task, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, t.Item9, t.Item10), CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token));

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, StructuredTask<TResult>> func)
    {
        // source is a value, so func runs eagerly with nothing to await first; return its handle
        // directly, matching the single-source value -> StructuredTask overload.
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult> func)
    {
        var source = await s.ConfigureAwait(false);
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, Task<TResult>> func)
    {
        var source = await s.ConfigureAwait(false);
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11).ConfigureAwait(false);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, t.Item9, t.Item10, t.Item11), new CancellationTokenSource());

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s.Task, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, t.Item9, t.Item10, t.Item11), CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token));

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, StructuredTask<TResult>> func)
    {
        // source is a value, so func runs eagerly with nothing to await first; return its handle
        // directly, matching the single-source value -> StructuredTask overload.
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult> func)
    {
        var source = await s.ConfigureAwait(false);
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, Task<TResult>> func)
    {
        var source = await s.ConfigureAwait(false);
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12).ConfigureAwait(false);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, t.Item9, t.Item10, t.Item11, t.Item12), new CancellationTokenSource());

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s.Task, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, t.Item9, t.Item10, t.Item11, t.Item12), CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token));

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, Task<TResult>> func)
    {
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, StructuredTask<TResult>> func)
    {
        // source is a value, so func runs eagerly with nothing to await first; return its handle
        // directly, matching the single-source value -> StructuredTask overload.
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult> func)
    {
        var source = await s.ConfigureAwait(false);
        return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static async StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, Task<TResult>> func)
    {
        var source = await s.ConfigureAwait(false);
        return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s.ConfigureAwait(false);
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13).ConfigureAwait(false);
        };

        return new StructuredTask<TResult>(impl(), s);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, t.Item9, t.Item10, t.Item11, t.Item12, t.Item13), new CancellationTokenSource());

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, StructuredTask<TResult>> func)
        => StructuredConcurrency.ChainTupleToStructured(s.Task, t => func(t.Item1, t.Item2, t.Item3, t.Item4, t.Item5, t.Item6, t.Item7, t.Item8, t.Item9, t.Item10, t.Item11, t.Item12, t.Item13), CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token));
}
