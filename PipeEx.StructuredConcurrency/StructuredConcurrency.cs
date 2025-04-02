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

    public static StructuredDeferedTask<TSource, TDeferd> Let<TSource, TDeferd>(this TSource source, Func<Task<TDeferd>> func) => 
        new StructuredDeferedTask<TSource, TDeferd>(Task.FromResult(source), func());

    public static StructuredDeferedTask<TSource, TDeferd> Let<TSource, TDeferd>(this TSource source, Func<TSource, Task<TDeferd>> func) => 
        new StructuredDeferedTask<TSource, TDeferd>(Task.FromResult(source), func(source));

    [OverloadResolutionPriority(1)]
    public static StructuredDeferedTask<TSource, TDeferd> Let<TSource, TDeferd>(this StructuredTask<TSource> source, Func<TSource, Task<TDeferd>> func)
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

        return new StructuredDeferedTask<TSource, TDeferd>(source, impl());
    }

    public static StructuredDeferedTask<TSource, TDeferd> Let<TSource, TDeferd>(this StructuredTask<TSource> source, Func<Task<TDeferd>> func) => 
        new StructuredDeferedTask<TSource, TDeferd>(source, func());

    public static StructuredDeferedTask<TSource, TDeferd1, TDeferd2> Let<TSource, TDeferd1, TDeferd2>(this StructuredDeferedTask<TSource, TDeferd1> source, Func<TSource, Task<TDeferd2>> func)
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

        return new StructuredDeferedTask<TSource, TDeferd1, TDeferd2>(source.Task, source.deferedTask1, impl());
    }

    public static StructuredDeferedTask<TSource, TDeferd1, TDeferd2> Let<TSource, TDeferd1, TDeferd2>(this StructuredDeferedTask<TSource, TDeferd1> source, Func<Task<TDeferd2>> func) => 
        new StructuredDeferedTask<TSource, TDeferd1, TDeferd2>(source.Task, source.deferedTask1, func());

    public static StructuredDeferedTask<TSource, TDeferd> Let<TSource, TDeferd>(this TSource source, Func<TSource, StructuredTask<TDeferd>> func)
    {
        var innerStructuredTask = func(source);
        var cts = new CancellationTokenSource();
        var registration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());

        var deferedCompletionSource = new TaskCompletionSource<TDeferd>();
        var wrapperTask = async () => {
            try {
                await Task.Yield();
                var result = await innerStructuredTask;
                deferedCompletionSource.SetResult(result);
            } catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token || ex.CancellationToken == innerStructuredTask.CancellationTokenSource.Token) {
                deferedCompletionSource.SetCanceled(cts.Token);
            } catch (Exception ex) {
                deferedCompletionSource.SetException(ex);
                cts.Cancel();
            } finally {
                registration.Dispose();
            }
        };
        wrapperTask();
        return new StructuredDeferedTask<TSource, TDeferd>(Task.FromResult(source), deferedCompletionSource.Task, cts);
    }

    [OverloadResolutionPriority(1)]
    public static StructuredDeferedTask<TSource, TDeferd> Let<TSource, TDeferd>(this StructuredTask<TSource> source, Func<TSource, StructuredTask<TDeferd>> func)
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(source.CancellationTokenSource.Token);
        var deferedTaskCompletionSource = new TaskCompletionSource<TDeferd>();
        StructuredTask<TDeferd>? innerStructuredTask = null;

        var deferedImpl = async () =>
        {
            try
            {
                await Task.Yield();
                var s = await source.Task;
                cts.Token.ThrowIfCancellationRequested();
                innerStructuredTask = func(s);
                using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());

                var result = await innerStructuredTask;
                deferedTaskCompletionSource.SetResult(result);
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token || (innerStructuredTask != null && ex.CancellationToken == innerStructuredTask.CancellationTokenSource.Token))
            {
               deferedTaskCompletionSource.SetCanceled(cts.Token);
            }
            catch (Exception ex)
            {
                deferedTaskCompletionSource.SetException(ex);
                cts.Cancel();
            }
        };
        deferedImpl();

        return new StructuredDeferedTask<TSource, TDeferd>(source.Task, deferedTaskCompletionSource.Task, cts);
    }

    public static StructuredDeferedTask<TSource, TDeferd> Let<TSource, TSource2, TDeferd>(this TSource source, Func<TSource, StructuredDeferedTask<TSource2, TDeferd>> func)
    {
        var innerDeferedTask = func(source);
        var cts = new CancellationTokenSource();
        var registration = cts.Token.Register(() => innerDeferedTask.CancellationTokenSource.Cancel());

        var deferedCompletionSource = new TaskCompletionSource<TDeferd>();
        var wrapperTask = async () => {
            try {
                await Task.Yield();
                var result = await innerDeferedTask.deferedTask1;
                deferedCompletionSource.SetResult(result);
            } catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token || ex.CancellationToken == innerDeferedTask.CancellationTokenSource.Token) {
                deferedCompletionSource.SetCanceled(cts.Token);
            } catch (Exception ex) {
                deferedCompletionSource.SetException(ex);
                cts.Cancel();
            } finally {
                registration.Dispose();
            }
        };
        wrapperTask();

        return new StructuredDeferedTask<TSource, TDeferd>(Task.FromResult(source), deferedCompletionSource.Task, cts);
    }


    public static StructuredDeferedTask<TResult, TDeferedSource> Await<TSource, TDeferedSource, TResult>(this StructuredDeferedTask<TSource, TDeferedSource> source, Func<TSource, TDeferedSource, TResult> func, [CallerArgumentExpression("func")] string propertyName = "")
    {
        source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        var impl = async () =>
        {
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var s = await source.Task;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var d = await source.deferedTask1;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = func(s, d);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        };

        var sdt = new StructuredDeferedTask<TResult, TDeferedSource>(impl(), source.deferedTask1, source.CancellationTokenSource);
        return sdt;
    }

    [OverloadResolutionPriority(1)]
    public static StructuredDeferedTask<TResult, TDeferedSource1, TDeferedSource2> Await<TSource, TDeferedSource1, TDeferedSource2, TResult>(this StructuredDeferedTask<TSource, TDeferedSource1, TDeferedSource2> source, Func<TSource, TDeferedSource1, TDeferedSource2, TResult> func, [CallerArgumentExpression("func")] string propertyName = "")
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
            var d1 = discard[1] ? default! : await source.deferedTask1;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var d2 = discard[2] ? default! : await source.deferedTask2;
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            var f = func(s, d1, d2);
            source.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            return f;
        };

        var sdt = new StructuredDeferedTask<TResult, TDeferedSource1, TDeferedSource2>(impl(), source.deferedTask1, source.deferedTask2, source.CancellationTokenSource);
        return sdt;
    }
}
