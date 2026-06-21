using System.Runtime.CompilerServices;

namespace PipeEx.StructuredConcurrency;

/// <summary>
/// Extension methods that enable pipe-style chaining (<c>I</c>), deferred parallel execution
/// (<c>Let</c>), and fan-in projection (<c>Await</c>) on <see cref="StructuredTask{T}"/>.
/// </summary>
public static class StructuredConcurrency
{
    /// <summary>
    /// Applies an asynchronous transformation to the source value, returning a <see cref="StructuredTask{TResult}"/>.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static async StructuredTask<TResult> I<TSource, TResult>(this TSource source, Func<TSource, Task<TResult>> func)
    {
        return await func(source).ConfigureAwait(false);
    }

    /// <summary>
    /// Invokes a factory that returns a <see cref="StructuredTask{TResult}"/> directly from the source value.
    /// </summary>
    public static StructuredTask<TResult> I<TSource, TResult>(this TSource source, Func<TSource, StructuredTask<TResult>> func)
    {
        return func(source);
    }

    /// <summary>
    /// Awaits the source task and applies a synchronous transformation to its result.
    /// </summary>
    public static async StructuredTask<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, TResult> func)
    {
        return func(await source.ConfigureAwait(false));
    }

    /// <summary>
    /// Chains a synchronous transformation onto a <see cref="StructuredTask{TSource}"/>, propagating cancellation.
    /// </summary>
    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, TResult> func)
        => new StructuredTask<TResult>(CheckedChain(source, s => Task.FromResult(func(s))), source);

    /// <summary>
    /// Awaits the source task and applies an asynchronous transformation to its result.
    /// </summary>
    public static async StructuredTask<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, Task<TResult>> func)
    {
        return await func(await source.ConfigureAwait(false)).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the source task and invokes a factory that returns a <see cref="StructuredTask{TResult}"/>.
    /// The single value/Task source -> StructuredTask overloads funnel through the same ChainTupleToStructured
    /// worker the generated tuple overloads use, so the await/cancellation/fault handling lives in one place.
    /// </summary>
    public static StructuredTask<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, StructuredTask<TResult>> func)
        => ChainTupleToStructured(source, func, new CancellationTokenSource());

    /// <summary>
    /// Chains an asynchronous transformation onto a <see cref="StructuredTask{TSource}"/>, propagating cancellation.
    /// </summary>
    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, Task<TResult>> func)
        => new StructuredTask<TResult>(CheckedChain(source, func), source);

    /// <summary>
    /// Chains a <see cref="StructuredTask{TResult}"/> factory onto a <see cref="StructuredTask{TSource}"/>, propagating cancellation.
    /// </summary>
    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, StructuredTask<TResult>> func)
        => ChainTupleToStructured(source.Task, func, CancellationTokenSource.CreateLinkedTokenSource(source.CancellationTokenSource.Token));

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

    /// <summary>
    /// Starts a deferred parallel task from a plain source value, using an independent factory (no source arg).
    /// </summary>
    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this TSource source, Func<Task<TDeferred>> func) =>
        new StructuredDeferredTask<TSource, TDeferred>(Task.FromResult(source), func());

    /// <summary>
    /// Starts a deferred parallel task from a plain source value, passing the source to the factory.
    /// </summary>
    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this TSource source, Func<TSource, Task<TDeferred>> func) =>
        new StructuredDeferredTask<TSource, TDeferred>(Task.FromResult(source), func(source));

    /// <summary>
    /// Chains a deferred parallel task onto a <see cref="StructuredTask{TSource}"/>, passing the source value to the factory.
    /// Cancellation from the source's scope propagates into the deferred task.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this StructuredTask<TSource> source, Func<TSource, Task<TDeferred>> func)
        => new StructuredDeferredTask<TSource, TDeferred>(source, CheckedChain(source, func));

    /// <summary>
    /// Chains a deferred parallel task onto a <see cref="StructuredTask{TSource}"/> using an independent factory (no source arg).
    /// </summary>
    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this StructuredTask<TSource> source, Func<Task<TDeferred>> func) =>
        new StructuredDeferredTask<TSource, TDeferred>(source, func());

    // Prioritised above the StructuredTask-source Let (which carries OverloadResolutionPriority(1) and
    // matches a StructuredDeferredTask via inheritance) so that chaining a source-arg Let onto a deferred
    // task keeps the earlier deferred instead of collapsing back to a single-deferred result.
    /// <summary>
    /// Adds a second deferred parallel task to an existing <see cref="StructuredDeferredTask{TSource, TDeferred1}"/>, passing the source value to the factory.
    /// </summary>
    [OverloadResolutionPriority(2)]
    public static StructuredDeferredTask<TSource, TDeferred1, TDeferred2> Let<TSource, TDeferred1, TDeferred2>(this StructuredDeferredTask<TSource, TDeferred1> source, Func<TSource, Task<TDeferred2>> func)
        => new StructuredDeferredTask<TSource, TDeferred1, TDeferred2>(source, CheckedChain(source, func));

    /// <summary>
    /// Adds a second deferred parallel task to an existing <see cref="StructuredDeferredTask{TSource, TDeferred1}"/> using an independent factory.
    /// </summary>
    public static StructuredDeferredTask<TSource, TDeferred1, TDeferred2> Let<TSource, TDeferred1, TDeferred2>(this StructuredDeferredTask<TSource, TDeferred1> source, Func<Task<TDeferred2>> func) =>
        new StructuredDeferredTask<TSource, TDeferred1, TDeferred2>(source, func());

    // async-let carries at most two deferred values (there is no three-deferred carrier and no four-arg
    // Await). A third Let would otherwise bind to the inherited two-deferred overload and silently drop a
    // deferred, so make it a loud compile error: Await the chain before adding another Let.
    /// <inheritdoc/>
    [OverloadResolutionPriority(3)]
    [Obsolete("async-let carries at most two deferred values; Await the chain before adding another Let.", true)]
    public static StructuredDeferredTask<TSource, TDeferred1, TDeferred2> Let<TSource, TDeferred1, TDeferred2, TDeferred3>(this StructuredDeferredTask<TSource, TDeferred1, TDeferred2> source, Func<TSource, Task<TDeferred3>> func)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    [OverloadResolutionPriority(3)]
    [Obsolete("async-let carries at most two deferred values; Await the chain before adding another Let.", true)]
    public static StructuredDeferredTask<TSource, TDeferred1, TDeferred2> Let<TSource, TDeferred1, TDeferred2, TDeferred3>(this StructuredDeferredTask<TSource, TDeferred1, TDeferred2> source, Func<Task<TDeferred3>> func)
        => throw new NotSupportedException();

    /// <summary>
    /// Starts a deferred parallel task from a plain source value using a factory that returns a <see cref="StructuredTask{TDeferred}"/>.
    /// The factory is invoked eagerly so synchronous exceptions surface at the call site.
    /// </summary>
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

    /// <summary>
    /// Chains a deferred parallel task onto a <see cref="StructuredTask{TSource}"/> using a factory that returns a <see cref="StructuredTask{TDeferred}"/>.
    /// Cancellation is observed before the factory runs.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TDeferred>(this StructuredTask<TSource> source, Func<TSource, StructuredTask<TDeferred>> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(source.CancellationTokenSource.Token);
        return new StructuredDeferredTask<TSource, TDeferred>(source.Task, ChainInnerStructured(source.Task, func, cts, deferStart: true), cts);
    }

    /// <summary>
    /// Wraps an existing <see cref="StructuredDeferredTask{TSource2, TDeferred}"/> inside a new deferred chain rooted at <paramref name="source"/>.
    /// The deferred result is bridged via a <see cref="TaskCompletionSource{TDeferred}"/> so cancellation flows correctly.
    /// </summary>
    public static StructuredDeferredTask<TSource, TDeferred> Let<TSource, TSource2, TDeferred>(this TSource source, Func<TSource, StructuredDeferredTask<TSource2, TDeferred>> func)
    {
        var innerDeferredTask = func(source);
        var cts = new CancellationTokenSource();
        var registration = cts.Token.Register(() => innerDeferredTask.CancellationTokenSource.Cancel());

        var deferredCompletionSource = new TaskCompletionSource<TDeferred>(TaskCreationOptions.RunContinuationsAsynchronously);
        var wrapperTask = async () => {
            try {
                await Task.Yield();
                var result = await innerDeferredTask.deferredTask1.ConfigureAwait(false);
                deferredCompletionSource.TrySetResult(result);
            } catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token || ex.CancellationToken == innerDeferredTask.CancellationTokenSource.Token) {
                deferredCompletionSource.TrySetCanceled(cts.Token);
            } catch (Exception ex) {
                deferredCompletionSource.TrySetException(ex);
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
