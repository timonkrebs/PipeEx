using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace PipeEx.StructuredConcurrency;

public static class StructuredConcurrency
{
    [OverloadResolutionPriority(1)]
    public static async StructuredTask<TResult> I<TSource, TResult>(this TSource source, Func<TSource, Task<TResult>> func)
    {
        return await func(source);
    }

    public static StructuredTask<TResult> I<TSource, TResult>(this TSource source, Func<TSource, StructuredTask<TResult>> func)
    {
        StructuredTask<TResult> structuredTask = default!;
        var impl = async () =>
        {
            structuredTask = func(source);
            return await structuredTask;
        };

        impl();
        return structuredTask;
    }

    public static async StructuredTask<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, TResult> func)
    {
        return func(await source);
    }

    public static StructuredTask<TResult> I<TSource, TResult>(this StructuredTask<TSource> source, Func<TSource, TResult> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var s = await source;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = func(s);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        };
        return new StructuredTask<TResult>(impl(), source);
    }

    public static async StructuredTask<TResult> I<TSource, TResult>(this Task<TSource> source, Func<TSource, Task<TResult>> func)
    {
        return await func(await source);
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
                    tcs.SetException(new OperationCanceledException());
                    return;
                }
                var innerStructuredTask = func(await source);

                using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                var result = await innerStructuredTask;

                tcs.SetResult(result);
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
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var s = await source;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = await func(s);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        };
        return new StructuredTask<TResult>(impl(), source.CancellationTokenSource);
    }

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
                    tcs.SetException(new OperationCanceledException());
                    return;
                }

                var innerStructuredTask = func(await source);
                using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                var result = await innerStructuredTask;

                tcs.SetResult(result);
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

    public static StructuredDeferredTask<TSource, TDeferd> Let<TSource, TDeferd>(this TSource source, Func<Task<TDeferd>> func) => 
        new StructuredDeferredTask<TSource, TDeferd>(Task.FromResult(source), func());

    public static StructuredDeferredTask<TSource, TDeferd> Let<TSource, TDeferd>(this TSource source, Func<TSource, Task<TDeferd>> func) => 
        new StructuredDeferredTask<TSource, TDeferd>(Task.FromResult(source), func(source));

    [OverloadResolutionPriority(1)]
    public static StructuredDeferredTask<TSource, TDeferd> Let<TSource, TDeferd>(this StructuredTask<TSource> source, Func<TSource, Task<TDeferd>> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var s = await source;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = await func(s);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        };

        return new StructuredDeferredTask<TSource, TDeferd>(source, impl());
    }

    public static StructuredDeferredTask<TSource, TDeferd> Let<TSource, TDeferd>(this StructuredTask<TSource> source, Func<Task<TDeferd>> func) => 
        new StructuredDeferredTask<TSource, TDeferd>(source, func());

    public static StructuredDeferredTask<TSource, TDeferd1, TDeferd2> Let<TSource, TDeferd1, TDeferd2>(this StructuredDeferredTask<TSource, TDeferd1> source, Func<TSource, Task<TDeferd2>> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var s = await source;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = await func(s);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        };

        return new StructuredDeferredTask<TSource, TDeferd1, TDeferd2>(source.Task, source.deferredTask1, impl());
    }

    public static StructuredDeferredTask<TSource, TDeferd1, TDeferd2> Let<TSource, TDeferd1, TDeferd2>(this StructuredDeferredTask<TSource, TDeferd1> source, Func<Task<TDeferd2>> func) => 
        new StructuredDeferredTask<TSource, TDeferd1, TDeferd2>(source.Task, source.deferredTask1, func());

    public static StructuredDeferredTask<TSource, TDeferd> Let<TSource, TDeferd>(this TSource source, Func<TSource, StructuredTask<TDeferd>> func)
    {
        var innerStructuredTask = func(source);
        var cts = new CancellationTokenSource();
        var registration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());

        var deferredCompletionSource = new TaskCompletionSource<TDeferd>();
        var wrapperTask = async () => {
            try {
                await Task.Yield();
                var result = await innerStructuredTask;
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
        return new StructuredDeferredTask<TSource, TDeferd>(Task.FromResult(source), deferredCompletionSource.Task, cts);
    }

    [OverloadResolutionPriority(1)]
    public static StructuredDeferredTask<TSource, TDeferd> Let<TSource, TDeferd>(this StructuredTask<TSource> source, Func<TSource, StructuredTask<TDeferd>> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(source.CancellationTokenSource.Token);
        var deferredTaskCompletionSource = new TaskCompletionSource<TDeferd>();
        StructuredTask<TDeferd>? innerStructuredTask = null;

        var deferredImpl = async () =>
        {
            try
            {
                await Task.Yield();
                var s = await source.Task;
                cts.Token.ThrowIfCancellationRequested();
                innerStructuredTask = func(s);
                using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());

                var result = await innerStructuredTask;
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

        return new StructuredDeferredTask<TSource, TDeferd>(source.Task, deferredTaskCompletionSource.Task, cts);
    }

    public static StructuredDeferredTask<TSource, TDeferd> Let<TSource, TSource2, TDeferd>(this TSource source, Func<TSource, StructuredDeferredTask<TSource2, TDeferd>> func)
    {
        var innerDeferredTask = func(source);
        var cts = new CancellationTokenSource();
        var registration = cts.Token.Register(() => innerDeferredTask.CancellationTokenSource.Cancel());

        var deferredCompletionSource = new TaskCompletionSource<TDeferd>();
        var wrapperTask = async () => {
            try {
                await Task.Yield();
                var result = await innerDeferredTask.deferredTask1;
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

        return new StructuredDeferredTask<TSource, TDeferd>(Task.FromResult(source), deferredCompletionSource.Task, cts);
    }


    public static StructuredDeferredTask<TResult, TDeferredSource> Await<TSource, TDeferredSource, TResult>(this StructuredDeferredTask<TSource, TDeferredSource> source, Func<TSource, TDeferredSource, TResult> func, [CallerArgumentExpression("func")] string propertyName = "")
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var s = await source.Task;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var d = await source.deferredTask1;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = func(s, d);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        };

        var sdt = new StructuredDeferredTask<TResult, TDeferredSource>(impl(), source.deferredTask1, source.CancellationTokenSource);
        return sdt;
    }

    [OverloadResolutionPriority(1)]
    public static StructuredDeferredTask<TResult, TDeferredSource1, TDeferredSource2> Await<TSource, TDeferredSource1, TDeferredSource2, TResult>(this StructuredDeferredTask<TSource, TDeferredSource1, TDeferredSource2> source, Func<TSource, TDeferredSource1, TDeferredSource2, TResult> func, [CallerArgumentExpression("func")] string propertyName = "")
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            Regex regex = new Regex(@"\(([^)]*)\)");
            Match match = regex.Match(propertyName);
            var discard = Regex.Split(match.Groups[1].Value, @",\s*").Select(x => x.Trim().StartsWith("_")).ToArray();

            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var s = discard[0] ? default! : await source.Task;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var d1 = discard[1] ? default! : await source.deferredTask1;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var d2 = discard[2] ? default! : await source.deferredTask2;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = func(s, d1, d2);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        };

        var sdt = new StructuredDeferredTask<TResult, TDeferredSource1, TDeferredSource2>(impl(), source.deferredTask1, source.deferredTask2, source.CancellationTokenSource);
        return sdt;
    }
}
