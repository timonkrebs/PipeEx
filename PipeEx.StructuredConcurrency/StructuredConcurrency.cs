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

    // The single value/Task source -> StructuredTask overloads funnel through the same ChainTupleToStructured
    // worker the generated tuple overloads use, so the await/cancellation/fault handling lives in one place.
    public static StructuredTask<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, StructuredTask<TResult>> func)
        => ChainTupleToStructured(source, func, new CancellationTokenSource());

    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, Task<TResult>> func)
        => new StructuredTask<TResult>(CheckedChain(source, func), source);

    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, StructuredTask<TResult>> func)
        => ChainTupleToStructured(source.Task, func, CancellationTokenSource.CreateLinkedTokenSource(source.CancellationTokenSource.Token));

    // --- Cancellation-aware I overloads: flow the pipe's token into the running job --------------
    // These mirror the awaiting I overloads above, but hand the carried CancellationToken to the job so
    // a running operation can observe cancellation and stop in flight, not only between stages. The
    // token still gates each await (via CheckedAwait), so a job that ignores the token it was handed is
    // nonetheless abandoned at its next await once cancellation is requested.

    // Value source: opens a pipe owning a fresh CancellationTokenSource and hands its token to the job.
    public static StructuredTask<TResult> I<TSource, TResult>(this TSource source, Func<TSource, CancellationToken, Task<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        return new StructuredTask<TResult>(Run(), cts);

        // async so a factory that throws synchronously faults the task (matching the awaiting value-source
        // overload) rather than throwing from this chaining call.
        async Task<TResult> Run() => await func(source, cts.Token).CheckedAwait(cts.Token).ConfigureAwait(false);
    }

    // Task source: awaits the source, then runs the cancellation-aware job under a fresh owned token.
    public static StructuredTask<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, CancellationToken, Task<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        return new StructuredTask<TResult>(Run(), cts);

        async Task<TResult> Run()
        {
            var s = await source.CheckedAwait(cts.Token).ConfigureAwait(false);
            return await func(s, cts.Token).CheckedAwait(cts.Token).ConfigureAwait(false);
        }
    }

    // StructuredTask source: shares the carried token (ownership transfer), so the job observes the same
    // cancellation that flows along the chain and cancelling the chain interrupts it while it runs.
    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, CancellationToken, Task<TResult>> func)
        => new StructuredTask<TResult>(CheckedChain(source, func), source);

    /// <summary>
    /// Observes <paramref name="ct"/> immediately before and after awaiting <paramref name="task"/> so an
    /// awaiting chain stops at the nearest await once cancellation is requested. Centralises the
    /// check-await-check pattern repeated across the awaiting <c>I</c> / <c>Let</c> / <c>Await</c> overloads.
    /// </summary>
    private static async Task<T> CheckedAwait<T>(this Task<T> task, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var result = await task.ConfigureAwait(false);
        ct.ThrowIfCancellationRequested();
        return result;
    }

    /// <summary>
    /// Awaits <paramref name="source"/> and applies <paramref name="map"/>, observing the source's
    /// cancellation token before and after each await. Shared by the <see cref="StructuredTask{T}"/>-source
    /// <c>I</c> and <c>Let</c> overloads so the cancellation-check pattern lives in one place. The up-front
    /// check runs synchronously so an already-cancelled source throws from the chaining call itself; the
    /// trailing check of the source await runs before <paramref name="map"/> is invoked, so cancellation
    /// observed after the source completes still skips the factory.
    /// </summary>
    private static Task<TResult> CheckedChain<TSource, TResult>(StructuredTask<TSource> source, Func<TSource, Task<TResult>> map)
    {
        var ct = source.CancellationTokenSource.Token;
        ct.ThrowIfCancellationRequested();
        return Impl();

        async Task<TResult> Impl()
        {
            var s = await source.Task.CheckedAwait(ct).ConfigureAwait(false);
            return await map(s).CheckedAwait(ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Cancellation-token-aware overload of
    /// <see cref="CheckedChain{TSource,TResult}(StructuredTask{TSource}, Func{TSource, Task{TResult}})"/>:
    /// hands the source's cancellation token to <paramref name="map"/> so the chained job can observe
    /// cancellation and stop while it runs, not only between stages. The token is still observed before
    /// and after each await, so the up-front (synchronous) check still throws from the chaining call on
    /// an already-cancelled source, and a job that ignores the token it was handed is still abandoned at
    /// its next await.
    /// </summary>
    private static Task<TResult> CheckedChain<TSource, TResult>(StructuredTask<TSource> source, Func<TSource, CancellationToken, Task<TResult>> map)
    {
        var ct = source.CancellationTokenSource.Token;
        ct.ThrowIfCancellationRequested();
        return Impl();

        async Task<TResult> Impl()
        {
            var s = await source.Task.CheckedAwait(ct).ConfigureAwait(false);
            return await map(s, ct).CheckedAwait(ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Links cancellation from <paramref name="cts"/> into the already-created
    /// <paramref name="innerStructuredTask"/>, awaits it, and surfaces its result, cancellation or fault
    /// on <paramref name="tcs"/>.
    /// </summary>
    private static async Task RunInnerStructured<TResult>(StructuredTask<TResult> innerStructuredTask, CancellationTokenSource cts, TaskCompletionSource<TResult> tcs)
    {
        try
        {
            using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
            var result = await innerStructuredTask.ConfigureAwait(false);
            tcs.TrySetResult(result);
        }
        catch (OperationCanceledException)
        {
            tcs.TrySetCanceled(innerStructuredTask.CancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }
    }

    /// <summary>
    /// Shared worker behind the source-awaiting <c>... -&gt; StructuredTask</c> chains (the scalar and
    /// generated tuple <c>I</c> overloads and the <c>StructuredTask</c>-source <c>Let</c>): awaits
    /// <paramref name="source"/>, then — only if cancellation has not been requested — invokes
    /// <paramref name="func"/> for the inner StructuredTask and runs it via <see cref="RunInnerStructured"/>.
    /// Source or factory cancellation (including a factory that throws <see cref="OperationCanceledException"/>)
    /// completes the task as canceled rather than faulted. When <paramref name="deferStart"/> is set the
    /// worker yields before reading <paramref name="source"/>, so a deferred (<c>Let</c>) chain can still be
    /// cancelled at the call site before the factory runs even when the source has already completed; the
    /// awaiting <c>I</c> overloads leave it unset and start synchronously as before.
    /// </summary>
    private static Task<TResult> ChainInnerStructured<TSource, TResult>(Task<TSource> source, Func<TSource, StructuredTask<TResult>> func, CancellationTokenSource cts, bool deferStart = false)
    {
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            if (deferStart)
                await Task.Yield();

            TSource value;
            try
            {
                value = await source.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                cts.Cancel();
                tcs.TrySetCanceled(cts.Token);
                return;
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                return;
            }

            StructuredTask<TResult> innerStructuredTask;
            try
            {
                cts.Token.ThrowIfCancellationRequested();
                innerStructuredTask = func(value);
            }
            catch (OperationCanceledException)
            {
                cts.Cancel();
                tcs.TrySetCanceled(cts.Token);
                return;
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                return;
            }

            await RunInnerStructured(innerStructuredTask, cts, tcs).ConfigureAwait(false);
        };
        impl();

        return tcs.Task;
    }

    /// <summary>
    /// Wraps <see cref="ChainInnerStructured"/> as a <see cref="StructuredTask{T}"/> carrying
    /// <paramref name="cts"/>. Used by the generated tuple overloads and the scalar <c>I</c> overloads.
    /// </summary>
    internal static StructuredTask<TResult> ChainTupleToStructured<TTuple, TResult>(Task<TTuple> source, Func<TTuple, StructuredTask<TResult>> func, CancellationTokenSource cts)
        => new StructuredTask<TResult>(ChainInnerStructured(source, func, cts), cts);

    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this TSource source, Func<Task<TDeferred>> func) =>
        new StructuredDeferredTask<TSource, TDeferred>(Task.FromResult(source), func());

    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this TSource source, Func<TSource, Task<TDeferred>> func) =>
        new StructuredDeferredTask<TSource, TDeferred>(Task.FromResult(source), func(source));

    // Cancellation-aware value-source Let: hand the carried token to the deferred work so cancelling the
    // chain interrupts it in flight. Unlike the token-free overload above — whose fresh
    // CancellationTokenSource the eagerly-started deferred never observes — the deferred here is handed
    // the token, so the chain's CancellationTokenSource actually reaches the running work.
    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this TSource source, Func<TSource, CancellationToken, Task<TDeferred>> func)
    {
        var cts = new CancellationTokenSource();
        try
        {
            return new StructuredDeferredTask<TSource, TDeferred>(Task.FromResult(source), func(source, cts.Token), cts);
        }
        catch
        {
            // A factory that throws synchronously surfaces at the call site (matching the token-free
            // value-source Let); dispose the token source created up-front so it does not leak.
            cts.Dispose();
            throw;
        }
    }

    [OverloadResolutionPriority(1)]
    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this StructuredTask<TSource> source, Func<TSource, Task<TDeferred>> func)
        => new StructuredDeferredTask<TSource, TDeferred>(source, CheckedChain(source, func));

    // Cancellation-aware StructuredTask-source Let: the deferred work is handed the source's token (via
    // the token-aware CheckedChain) so cancellation reaches it while it runs. Priority mirrors the
    // token-free overload above so a deferred-source Let still wins when chaining onto a deferred task.
    [OverloadResolutionPriority(1)]
    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this StructuredTask<TSource> source, Func<TSource, CancellationToken, Task<TDeferred>> func)
        => new StructuredDeferredTask<TSource, TDeferred>(source, CheckedChain(source, func));

    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this StructuredTask<TSource> source, Func<Task<TDeferred>> func) => 
        new StructuredDeferredTask<TSource, TDeferred>(source, func());

    // Prioritised above the StructuredTask-source Let (which carries OverloadResolutionPriority(1) and
    // matches a StructuredDeferredTask via inheritance) so that chaining a source-arg Let onto a deferred
    // task keeps the earlier deferred instead of collapsing back to a single-deferred result.
    [OverloadResolutionPriority(2)]
    public static StructuredDeferredTask<TSource, TDeferred1, TDeferred2> Let<TSource, TDeferred1, TDeferred2>(this StructuredDeferredTask<TSource, TDeferred1> source, Func<TSource, Task<TDeferred2>> func)
        => new StructuredDeferredTask<TSource, TDeferred1, TDeferred2>(source, CheckedChain(source, func));

    // Cancellation-aware deferred-source Let: extends the chain with a second deferred that is handed the
    // chain's token, so cancellation reaches the newly added stage while it runs.
    [OverloadResolutionPriority(2)]
    public static StructuredDeferredTask<TSource, TDeferred1, TDeferred2> Let<TSource, TDeferred1, TDeferred2>(this StructuredDeferredTask<TSource, TDeferred1> source, Func<TSource, CancellationToken, Task<TDeferred2>> func)
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

    // The cancellation-aware third Let must be a loud compile error too, otherwise it would bind to the
    // two-deferred token overload (via inheritance) and silently drop a deferred.
    [OverloadResolutionPriority(3)]
    [Obsolete("async-let carries at most two deferred values; Await the chain before adding another Let.", true)]
    public static StructuredDeferredTask<TSource, TDeferred1, TDeferred2> Let<TSource, TDeferred1, TDeferred2, TDeferred3>(this StructuredDeferredTask<TSource, TDeferred1, TDeferred2> source, Func<TSource, CancellationToken, Task<TDeferred3>> func)
        => throw new NotSupportedException();

    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this TSource source, Func<TSource, StructuredTask<TDeferred>> func)
    {
        // Invoke the factory eagerly so its synchronous exceptions surface at the Let call site, matching
        // the Task-returning value-source Let overload.
        var innerStructuredTask = func(source);
        var cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<TDeferred>(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = RunInnerStructured(innerStructuredTask, cts, tcs);
        return new StructuredDeferredTask<TSource, TDeferred>(Task.FromResult(source), tcs.Task, cts);
    }

    [OverloadResolutionPriority(1)]
    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this StructuredTask<TSource> source, Func<TSource, StructuredTask<TDeferred>> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(source.CancellationTokenSource.Token);
        return new StructuredDeferredTask<TSource, TDeferred>(source.Task, ChainInnerStructured(source.Task, func, cts, deferStart: true), cts);
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
        var ct = source.CancellationTokenSource.Token;
        ct.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            var s = await source.Task.CheckedAwait(ct).ConfigureAwait(false);
            var d = await source.deferredTask1.CheckedAwait(ct).ConfigureAwait(false);
            var f = func(s, d);
            ct.ThrowIfCancellationRequested();
            return f;
        };

        return new StructuredDeferredTask<TResult, TDeferredSource>(impl(), source.deferredTask1, source.CancellationTokenSource);
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
        var ct = source.CancellationTokenSource.Token;
        ct.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            var s = await source.Task.CheckedAwait(ct).ConfigureAwait(false);
            var d1 = await source.deferredTask1.CheckedAwait(ct).ConfigureAwait(false);
            var d2 = await source.deferredTask2.CheckedAwait(ct).ConfigureAwait(false);
            var f = func(s, d1, d2);
            ct.ThrowIfCancellationRequested();
            return f;
        };

        return new StructuredDeferredTask<TResult, TDeferredSource1, TDeferredSource2>(impl(), source.deferredTask1, source.deferredTask2, source.CancellationTokenSource);
    }
}
