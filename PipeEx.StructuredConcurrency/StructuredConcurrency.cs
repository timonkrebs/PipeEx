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
        => ChainStructuredToStructured(source, func);

    // --- Cancellation-aware I overloads: flow the pipe's token into the running job --------------
    // These mirror the awaiting I overloads above, but hand the carried CancellationToken to the job so
    // a running operation can observe cancellation and stop in flight, not only between stages. The
    // token still gates each await (via CheckedAwait), so even a job that ignores the token it was
    // handed cannot complete the chain successfully: the chain waits for the job to finish, then the
    // checkpoint after it discards the result and completes the chain as canceled.

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
    /// Internal (rather than private) so the generated cancellation-aware tuple <c>I</c> overloads can
    /// route through the same checked path instead of bypassing cancellation with plain awaits.
    /// </summary>
    internal static async Task<T> CheckedAwait<T>(this Task<T> task, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var result = await task.ConfigureAwait(false);
        ct.ThrowIfCancellationRequested();
        return result;
    }

    /// <summary>
    /// Attaches a continuation that reads (and thereby observes) the task's exception, so a deferred
    /// result abandoned by an earlier failure cannot raise <see cref="TaskScheduler.UnobservedTaskException"/>.
    /// </summary>
    private static void Observe(Task task) =>
        _ = task.ContinueWith(static t => _ = t.Exception, CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

    /// <summary>
    /// Awaits <paramref name="source"/> and applies <paramref name="map"/>, observing the source's
    /// cancellation token before and after each await. Shared by the <see cref="StructuredTask{T}"/>-source
    /// <c>I</c> and <c>Let</c> overloads so the cancellation-check pattern lives in one place. The up-front
    /// check runs synchronously so an already-cancelled source throws from the chaining call itself; the
    /// trailing check of the source await runs before <paramref name="map"/> is invoked, so cancellation
    /// observed after the source completes still skips the factory. NOTE: the generated
    /// <c>StructuredTask</c>-tuple <c>I</c> overloads (TupleDestructuringGenerator) inline this pattern —
    /// keep them in sync when changing it.
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
    /// an already-cancelled source, and even a job that ignores the token it was handed cannot complete
    /// the chain successfully — the checkpoint after it discards the result and completes the chain as
    /// canceled. NOTE: the generated <c>StructuredTask</c>-tuple token <c>I</c> overloads
    /// (TupleDestructuringGenerator) inline this pattern — keep them in sync when changing it.
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
            // The inner pipe may ignore the cancellation relayed into it (e.g. a plain async
            // StructuredTask method whose body never observes its token) and complete normally; a chain
            // whose CancellationTokenSource was cancelled must still complete as canceled, matching the
            // trailing-checkpoint contract of the Task-returning overloads.
            if (cts.IsCancellationRequested)
                tcs.TrySetCanceled(cts.Token);
            else
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

    /// <summary>
    /// Chains a StructuredTask-returning factory onto a <see cref="StructuredTask{T}"/> source, sharing
    /// the source's <see cref="CancellationTokenSource"/> (with ownership transfer) rather than linking a
    /// child source. Linked sources propagate cancellation one way only (parent to child), so a linked
    /// chain could never cancel upstream stages: cancelling the returned pipe would leave a token-aware
    /// upstream job running — and hang the pipe outright when that job only completes via its token.
    /// Sharing keeps the whole pipe on one CancellationTokenSource, matching the Task-returning overloads.
    /// Used by the scalar and generated tuple <c>StructuredTask -&gt; StructuredTask</c> <c>I</c> overloads.
    /// </summary>
    internal static StructuredTask<TResult> ChainStructuredToStructured<TSource, TResult>(StructuredTask<TSource> source, Func<TSource, StructuredTask<TResult>> func)
    {
        var cts = source.CancellationTokenSource;
        var result = new StructuredTask<TResult>(ChainInnerStructured(source.Task, func, cts), cts);
        source.MustHandleDisposing = false;
        return result;
    }

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

    // A StructuredTask-returning factory chained onto a deferred task must also keep the earlier
    // deferred: without this overload it binds the StructuredTask-source Let via inheritance and the
    // earlier deferred is silently dropped (never awaited). Mirrors the Task-returning deferred-source
    // overload above; the chain's CancellationTokenSource is shared through the two-deferred carrier.
    [OverloadResolutionPriority(2)]
    public static StructuredDeferredTask<TSource, TDeferred1, TDeferred2> Let<TSource, TDeferred1, TDeferred2>(this StructuredDeferredTask<TSource, TDeferred1> source, Func<TSource, StructuredTask<TDeferred2>> func)
    {
        var cts = source.CancellationTokenSource;
        cts.Token.ThrowIfCancellationRequested();
        return new StructuredDeferredTask<TSource, TDeferred1, TDeferred2>(source, ChainInnerStructured(source.Task, func, cts, deferStart: true));
    }

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

    // ...and likewise for a StructuredTask-returning factory, which would otherwise bind to the
    // two-deferred structured overload (via inheritance) and silently drop both deferreds.
    [OverloadResolutionPriority(3)]
    [Obsolete("async-let carries at most two deferred values; Await the chain before adding another Let.", true)]
    public static StructuredDeferredTask<TSource, TDeferred1, TDeferred2> Let<TSource, TDeferred1, TDeferred2, TDeferred3>(this StructuredDeferredTask<TSource, TDeferred1, TDeferred2> source, Func<TSource, StructuredTask<TDeferred3>> func)
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
        // Share the source's CancellationTokenSource (with ownership transfer) instead of linking a
        // child source: linking is one-way, so cancelling the returned chain could never reach the
        // upstream stages (see ChainStructuredToStructured).
        var cts = source.CancellationTokenSource;
        cts.Token.ThrowIfCancellationRequested();
        var result = new StructuredDeferredTask<TSource, TDeferred>(source.Task, ChainInnerStructured(source.Task, func, cts, deferStart: true), cts);
        source.MustHandleDisposing = false;
        return result;
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
            } catch (OperationCanceledException) {
                // Any cancellation completes the deferred as canceled regardless of which token the
                // exception carries (a foreign token, e.g. an HttpClient timeout, is still a
                // cancellation), matching RunInnerStructured and ChainInnerStructured.
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
            try
            {
                var s = await source.Task.CheckedAwait(ct).ConfigureAwait(false);
                var d = await source.deferredTask1.CheckedAwait(ct).ConfigureAwait(false);
                var f = func(s, d);
                ct.ThrowIfCancellationRequested();
                return f;
            }
            catch
            {
                // The first failure (or cancellation) propagates, but a deferred result this projection
                // never reached must still be observed so its fault cannot surface as an
                // UnobservedTaskException. Observing an already-consumed task is a no-op.
                Observe(source.deferredTask1);
                throw;
            }
        };

        var result = new StructuredDeferredTask<TResult, TDeferredSource>(impl(), source.deferredTask1, source.CancellationTokenSource);
        source.MustHandleDisposing = false;
        return result;
    }

    /// <summary>
    /// Awaits the source and both deferred results and projects them with <paramref name="func"/>.
    /// On success every deferred result is awaited; when an earlier await fails, the first failure
    /// propagates and the deferred results the projection never reached are still observed (their
    /// faults cannot surface as unobserved task exceptions). The projection is free to ignore any
    /// argument it does not need (e.g. by using a discard parameter).
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
            try
            {
                var s = await source.Task.CheckedAwait(ct).ConfigureAwait(false);
                var d1 = await source.deferredTask1.CheckedAwait(ct).ConfigureAwait(false);
                var d2 = await source.deferredTask2.CheckedAwait(ct).ConfigureAwait(false);
                var f = func(s, d1, d2);
                ct.ThrowIfCancellationRequested();
                return f;
            }
            catch
            {
                // See the two-argument Await: deferred results the projection never reached must still
                // be observed so their faults cannot surface as UnobservedTaskException.
                Observe(source.deferredTask1);
                Observe(source.deferredTask2);
                throw;
            }
        };

        var result = new StructuredDeferredTask<TResult, TDeferredSource1, TDeferredSource2>(impl(), source.deferredTask1, source.deferredTask2, source.CancellationTokenSource);
        source.MustHandleDisposing = false;
        return result;
    }

    // A two-argument projection on a two-deferred chain would bind the inherited two-argument Await and
    // silently orphan the second deferred (never awaited, its fault unobserved), so make it a loud
    // compile error: use the three-argument projection and discard what you do not need.
    [OverloadResolutionPriority(2)]
    [Obsolete("this chain carries two deferred values; use the three-argument Await projection and discard any value you do not need.", true)]
    public static StructuredDeferredTask<TResult, TDeferredSource1> Await<TSource, TDeferredSource1, TDeferredSource2, TResult>(this StructuredDeferredTask<TSource, TDeferredSource1, TDeferredSource2> source, Func<TSource, TDeferredSource1, TResult> func)
        => throw new NotSupportedException();
}
