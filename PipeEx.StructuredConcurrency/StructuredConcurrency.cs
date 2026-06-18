using System.Runtime.CompilerServices;

namespace PipeEx.StructuredConcurrency;

public static class StructuredConcurrency
{
    [OverloadResolutionPriority(1)]
    public static async StructuredTask<TResult> I<TSource, TResult>(this TSource source, Func<TSource, Task<TResult>> func)
    {
        return await func(source).ConfigureAwait(false);
    }

    public static StructuredTask<TResult> I<TSource, TResult>(this TSource source, Func<TSource, StructuredTask<TResult>> func)
    {
        return func(source);
    }

    public static async StructuredTask<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, TResult> func)
    {
        return func(await source.ConfigureAwait(false));
    }

    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, TResult> func)
        => new StructuredTask<TResult>(CheckedChain(source, s => Task.FromResult(func(s))), source);

    public static async StructuredTask<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, Task<TResult>> func)
    {
        return await func(await source.ConfigureAwait(false)).ConfigureAwait(false);
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
                    cts.Cancel();
                    tcs.SetCanceled(cts.Token);
                    return;
                }
                var innerStructuredTask = func(await source.ConfigureAwait(false));

                using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                var result = await innerStructuredTask.ConfigureAwait(false);

                tcs.SetResult(result);
            }
            catch (OperationCanceledException oce)
            {
                cts.Cancel();
                tcs.TrySetCanceled(oce.CancellationToken);
            }
            catch (Exception ex)
            {
                cts.Cancel();
                tcs.SetException(ex);
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, Task<TResult>> func)
        => new StructuredTask<TResult>(CheckedChain(source, func), source);

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
                    cts.Cancel();
                    tcs.SetCanceled(cts.Token);
                    return;
                }

                var innerStructuredTask = func(await source.ConfigureAwait(false));
                using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                var result = await innerStructuredTask.ConfigureAwait(false);

                tcs.SetResult(result);
            }
            catch (OperationCanceledException oce)
            {
                cts.Cancel();
                tcs.TrySetCanceled(oce.CancellationToken);
            }
            catch (Exception ex)
            {
                cts.Cancel();
                tcs.SetException(ex);
            }
        };
        impl();
        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    /// <summary>
    /// Awaits <paramref name="source"/> and applies <paramref name="map"/>, observing the source's
    /// cancellation token before and after each await. Shared by the <see cref="StructuredTask{T}"/>-source
    /// <c>I</c> and <c>Let</c> overloads so the cancellation-check pattern lives in one place.
    /// </summary>
    private static Task<TResult> CheckedChain<TSource, TResult>(StructuredTask<TSource> source, Func<TSource, Task<TResult>> map)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        return Impl();

        async Task<TResult> Impl()
        {
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var s = await source.ConfigureAwait(false);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = await map(s).ConfigureAwait(false);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        }
    }

    /// <summary>
    /// Shared implementation for the generated tuple <c>... -&gt; StructuredTask</c> overloads: awaits
    /// <paramref name="source"/>, invokes <paramref name="func"/> to get the inner StructuredTask, links
    /// cancellation through <paramref name="cts"/>, and surfaces its result, cancellation or fault.
    /// </summary>
    internal static StructuredTask<TResult> ChainTupleToStructured<TTuple, TResult>(Task<TTuple> source, Func<TTuple, StructuredTask<TResult>> func, CancellationTokenSource cts)
    {
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                TTuple value;
                try
                {
                    value = await source.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    cts.Cancel();
                    tcs.SetCanceled(cts.Token);
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(value);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask.ConfigureAwait(false);
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
                tcs.TrySetException(ex);
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this TSource source, Func<Task<TDeferred>> func) =>
        new StructuredDeferredTask<TSource, TDeferred>(Task.FromResult(source), func());

    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this TSource source, Func<TSource, Task<TDeferred>> func) => 
        new StructuredDeferredTask<TSource, TDeferred>(Task.FromResult(source), func(source));

    [OverloadResolutionPriority(1)]
    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this StructuredTask<TSource> source, Func<TSource, Task<TDeferred>> func)
        => new StructuredDeferredTask<TSource, TDeferred>(source, CheckedChain(source, func));

    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this StructuredTask<TSource> source, Func<Task<TDeferred>> func) => 
        new StructuredDeferredTask<TSource, TDeferred>(source, func());

    // Prioritised above the StructuredTask-source Let (which carries OverloadResolutionPriority(1) and
    // matches a StructuredDeferredTask via inheritance) so that chaining a source-arg Let onto a deferred
    // task keeps the earlier deferred instead of collapsing back to a single-deferred result.
    [OverloadResolutionPriority(2)]
    public static StructuredDeferredTask<TSource, TDeferred1, TDeferred2> Let<TSource, TDeferred1, TDeferred2>(this StructuredDeferredTask<TSource, TDeferred1> source, Func<TSource, Task<TDeferred2>> func)
        => new StructuredDeferredTask<TSource, TDeferred1, TDeferred2>(source, CheckedChain(source, func));

    public static StructuredDeferredTask<TSource, TDeferred1, TDeferred2> Let<TSource, TDeferred1, TDeferred2>(this StructuredDeferredTask<TSource, TDeferred1> source, Func<Task<TDeferred2>> func) =>
        new StructuredDeferredTask<TSource, TDeferred1, TDeferred2>(source, func());

    // async-let carries at most two deferred values (there is no three-deferred carrier and no four-arg
    // Await). A third Let would otherwise bind to the inherited two-deferred overload and silently drop a
    // deferred, so make it a loud compile error: Await the chain before adding another Let.
    [OverloadResolutionPriority(3)]
    [Obsolete("async-let carries at most two deferred values; Await the chain before adding another Let.", true)]
    public static StructuredDeferredTask<TSource, TDeferred1, TDeferred2> Let<TSource, TDeferred1, TDeferred2, TDeferred3>(this StructuredDeferredTask<TSource, TDeferred1, TDeferred2> source, Func<TSource, Task<TDeferred3>> func)
        => throw new NotSupportedException();

    [OverloadResolutionPriority(3)]
    [Obsolete("async-let carries at most two deferred values; Await the chain before adding another Let.", true)]
    public static StructuredDeferredTask<TSource, TDeferred1, TDeferred2> Let<TSource, TDeferred1, TDeferred2, TDeferred3>(this StructuredDeferredTask<TSource, TDeferred1, TDeferred2> source, Func<Task<TDeferred3>> func)
        => throw new NotSupportedException();

    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this TSource source, Func<TSource, StructuredTask<TDeferred>> func)
    {
        var innerStructuredTask = func(source);
        var cts = new CancellationTokenSource();
        var registration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());

        var deferredCompletionSource = new TaskCompletionSource<TDeferred>();
        var wrapperTask = async () => {
            try {
                await Task.Yield();
                var result = await innerStructuredTask.ConfigureAwait(false);
                deferredCompletionSource.SetResult(result);
            } catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token || ex.CancellationToken == innerStructuredTask.CancellationTokenSource.Token) {
                deferredCompletionSource.SetCanceled(cts.Token);
            } catch (Exception ex) {
                deferredCompletionSource.SetException(ex);
                cts.Cancel();
            } finally {
                registration.Dispose();
            }
        };
        wrapperTask();
        return new StructuredDeferredTask<TSource, TDeferred>(Task.FromResult(source), deferredCompletionSource.Task, cts);
    }

    [OverloadResolutionPriority(1)]
    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this StructuredTask<TSource> source, Func<TSource, StructuredTask<TDeferred>> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(source.CancellationTokenSource.Token);
        var deferredTaskCompletionSource = new TaskCompletionSource<TDeferred>();
        StructuredTask<TDeferred>? innerStructuredTask = null;

        var deferredImpl = async () =>
        {
            try
            {
                await Task.Yield();
                var s = await source.Task.ConfigureAwait(false);
                cts.Token.ThrowIfCancellationRequested();
                innerStructuredTask = func(s);
                using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());

                var result = await innerStructuredTask.ConfigureAwait(false);
                deferredTaskCompletionSource.SetResult(result);
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token || (innerStructuredTask != null && ex.CancellationToken == innerStructuredTask.CancellationTokenSource.Token))
            {
               deferredTaskCompletionSource.SetCanceled(cts.Token);
            }
            catch (Exception ex)
            {
                deferredTaskCompletionSource.SetException(ex);
                cts.Cancel();
            }
        };
        deferredImpl();

        return new StructuredDeferredTask<TSource, TDeferred>(source.Task, deferredTaskCompletionSource.Task, cts);
    }

    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TSource2, TDeferred>(this TSource source, Func<TSource, StructuredDeferredTask<TSource2, TDeferred>> func)
    {
        var innerDeferredTask = func(source);
        var cts = new CancellationTokenSource();
        var registration = cts.Token.Register(() => innerDeferredTask.CancellationTokenSource.Cancel());

        var deferredCompletionSource = new TaskCompletionSource<TDeferred>();
        var wrapperTask = async () => {
            try {
                await Task.Yield();
                var result = await innerDeferredTask.deferredTask1.ConfigureAwait(false);
                deferredCompletionSource.SetResult(result);
            } catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token || ex.CancellationToken == innerDeferredTask.CancellationTokenSource.Token) {
                deferredCompletionSource.SetCanceled(cts.Token);
            } catch (Exception ex) {
                deferredCompletionSource.SetException(ex);
                cts.Cancel();
            } finally {
                registration.Dispose();
            }
        };
        wrapperTask();

        return new StructuredDeferredTask<TSource, TDeferred>(Task.FromResult(source), deferredCompletionSource.Task, cts);
    }


    /// <summary>
    /// Awaits the source and the deferred result and projects them with <paramref name="func"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TDeferredSource">The type of the deferred result.</typeparam>
    /// <typeparam name="TResult">The type produced by the projection.</typeparam>
    /// <param name="source">The deferred task carrying the source and the deferred result.</param>
    /// <param name="func">The projection applied to the source and the deferred result.</param>
    public static StructuredDeferredTask<TResult, TDeferredSource> Await<TSource, TDeferredSource, TResult>(this StructuredDeferredTask<TSource, TDeferredSource> source, Func<TSource, TDeferredSource, TResult> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var s = await source.Task.ConfigureAwait(false);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var d = await source.deferredTask1.ConfigureAwait(false);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = func(s, d);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        };

        var sdt = new StructuredDeferredTask<TResult, TDeferredSource>(impl(), source.deferredTask1, source.CancellationTokenSource);
        return sdt;
    }

    /// <summary>
    /// Awaits the source and both deferred results and projects them with <paramref name="func"/>.
    /// Every deferred result is awaited so that all of their exceptions are observed; the projection
    /// is free to ignore any argument it does not need (e.g. by using a discard parameter).
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TDeferredSource1">The type of the first deferred result.</typeparam>
    /// <typeparam name="TDeferredSource2">The type of the second deferred result.</typeparam>
    /// <typeparam name="TResult">The type produced by the projection.</typeparam>
    /// <param name="source">The deferred task carrying the source and the two deferred results.</param>
    /// <param name="func">The projection applied to the source and the two deferred results.</param>
    [OverloadResolutionPriority(1)]
    public static StructuredDeferredTask<TResult, TDeferredSource1, TDeferredSource2> Await<TSource, TDeferredSource1, TDeferredSource2, TResult>(this StructuredDeferredTask<TSource, TDeferredSource1, TDeferredSource2> source, Func<TSource, TDeferredSource1, TDeferredSource2, TResult> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var s = await source.Task.ConfigureAwait(false);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var d1 = await source.deferredTask1.ConfigureAwait(false);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var d2 = await source.deferredTask2.ConfigureAwait(false);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = func(s, d1, d2);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        };

        var sdt = new StructuredDeferredTask<TResult, TDeferredSource1, TDeferredSource2>(impl(), source.deferredTask1, source.deferredTask2, source.CancellationTokenSource);
        return sdt;
    }
}
