namespace PipeEx.StructuredConcurrency;

public static class TupleDestructuring
{

    public static StructuredTask<TResult> I<TSource1, TSource2, TResult>(this (TSource1, TSource2) source, Func<TSource1, TSource2, Task<TResult>> func) => 
        new StructuredTask<TResult>(func(source.Item1, source.Item2));

    public static StructuredTask<TResult> I<TSource1, TSource2, TResult>(this (TSource1, TSource2) source, Func<TSource1, TSource2, StructuredTask<TResult>> func)
    {
        // This works because the structuredTask is assigned befor the await is hit.
        StructuredTask<TResult> structuredTask = default!;
        var impl = async () =>
        {
            structuredTask = func(source.Item1, source.Item2);
            return await structuredTask;
        };

        return new StructuredTask<TResult>(impl(), structuredTask.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TResult>(this Task<(TSource1, TSource2)> s, Func<TSource1, TSource2, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TResult>(this StructuredTask<(TSource1, TSource2)> s, Func<TSource1, TSource2, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TResult>(this Task<(TSource1, TSource2)> s, Func<TSource1, TSource2, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TResult>(this StructuredTask<(TSource1, TSource2)> s, Func<TSource1, TSource2, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

     public static StructuredTask<TResult> I<TSource1, TSource2, TResult>(this Task<(TSource1, TSource2)> s, Func<TSource1, TSource2, StructuredTask<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TResult>(this StructuredTask<(TSource1, TSource2)> s, Func<TSource1, TSource2, StructuredTask<TResult>> func)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TResult>(this (TSource1, TSource2, TSource3) source, Func<TSource1, TSource2, TSource3, Task<TResult>> func) => 
        new StructuredTask<TResult>(func(source.Item1, source.Item2, source.Item3));

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TResult>(this (TSource1, TSource2, TSource3) source, Func<TSource1, TSource2, TSource3, StructuredTask<TResult>> func)
    {
        // This works because the structuredTask is assigned befor the await is hit.
        StructuredTask<TResult> structuredTask = default!;
        var impl = async () =>
        {
            structuredTask = func(source.Item1, source.Item2, source.Item3);
            return await structuredTask;
        };

        return new StructuredTask<TResult>(impl(), structuredTask.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TResult>(this Task<(TSource1, TSource2, TSource3)> s, Func<TSource1, TSource2, TSource3, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TResult>(this StructuredTask<(TSource1, TSource2, TSource3)> s, Func<TSource1, TSource2, TSource3, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TResult>(this Task<(TSource1, TSource2, TSource3)> s, Func<TSource1, TSource2, TSource3, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TResult>(this StructuredTask<(TSource1, TSource2, TSource3)> s, Func<TSource1, TSource2, TSource3, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

     public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TResult>(this Task<(TSource1, TSource2, TSource3)> s, Func<TSource1, TSource2, TSource3, StructuredTask<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TResult>(this StructuredTask<(TSource1, TSource2, TSource3)> s, Func<TSource1, TSource2, TSource3, StructuredTask<TResult>> func)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this (TSource1, TSource2, TSource3, TSource4) source, Func<TSource1, TSource2, TSource3, TSource4, Task<TResult>> func) => 
        new StructuredTask<TResult>(func(source.Item1, source.Item2, source.Item3, source.Item4));

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this (TSource1, TSource2, TSource3, TSource4) source, Func<TSource1, TSource2, TSource3, TSource4, StructuredTask<TResult>> func)
    {
        // This works because the structuredTask is assigned befor the await is hit.
        StructuredTask<TResult> structuredTask = default!;
        var impl = async () =>
        {
            structuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4);
            return await structuredTask;
        };

        return new StructuredTask<TResult>(impl(), structuredTask.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4)> s, Func<TSource1, TSource2, TSource3, TSource4, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4)> s, Func<TSource1, TSource2, TSource3, TSource4, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4)> s, Func<TSource1, TSource2, TSource3, TSource4, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4)> s, Func<TSource1, TSource2, TSource3, TSource4, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

     public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4)> s, Func<TSource1, TSource2, TSource3, TSource4, StructuredTask<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4)> s, Func<TSource1, TSource2, TSource3, TSource4, StructuredTask<TResult>> func)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, Task<TResult>> func) => 
        new StructuredTask<TResult>(func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5));

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, StructuredTask<TResult>> func)
    {
        // This works because the structuredTask is assigned befor the await is hit.
        StructuredTask<TResult> structuredTask = default!;
        var impl = async () =>
        {
            structuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5);
            return await structuredTask;
        };

        return new StructuredTask<TResult>(impl(), structuredTask.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

     public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, StructuredTask<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, StructuredTask<TResult>> func)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, Task<TResult>> func) => 
        new StructuredTask<TResult>(func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6));

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, StructuredTask<TResult>> func)
    {
        // This works because the structuredTask is assigned befor the await is hit.
        StructuredTask<TResult> structuredTask = default!;
        var impl = async () =>
        {
            structuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6);
            return await structuredTask;
        };

        return new StructuredTask<TResult>(impl(), structuredTask.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

     public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, StructuredTask<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, StructuredTask<TResult>> func)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, Task<TResult>> func) => 
        new StructuredTask<TResult>(func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7));

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, StructuredTask<TResult>> func)
    {
        // This works because the structuredTask is assigned befor the await is hit.
        StructuredTask<TResult> structuredTask = default!;
        var impl = async () =>
        {
            structuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7);
            return await structuredTask;
        };

        return new StructuredTask<TResult>(impl(), structuredTask.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

     public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, StructuredTask<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, StructuredTask<TResult>> func)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, Task<TResult>> func) => 
        new StructuredTask<TResult>(func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8));

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, StructuredTask<TResult>> func)
    {
        // This works because the structuredTask is assigned befor the await is hit.
        StructuredTask<TResult> structuredTask = default!;
        var impl = async () =>
        {
            structuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8);
            return await structuredTask;
        };

        return new StructuredTask<TResult>(impl(), structuredTask.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

     public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, StructuredTask<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, StructuredTask<TResult>> func)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, Task<TResult>> func) => 
        new StructuredTask<TResult>(func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9));

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, StructuredTask<TResult>> func)
    {
        // This works because the structuredTask is assigned befor the await is hit.
        StructuredTask<TResult> structuredTask = default!;
        var impl = async () =>
        {
            structuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9);
            return await structuredTask;
        };

        return new StructuredTask<TResult>(impl(), structuredTask.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

     public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, StructuredTask<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, StructuredTask<TResult>> func)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, Task<TResult>> func) => 
        new StructuredTask<TResult>(func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10));

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, StructuredTask<TResult>> func)
    {
        // This works because the structuredTask is assigned befor the await is hit.
        StructuredTask<TResult> structuredTask = default!;
        var impl = async () =>
        {
            structuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10);
            return await structuredTask;
        };

        return new StructuredTask<TResult>(impl(), structuredTask.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

     public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, StructuredTask<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, StructuredTask<TResult>> func)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, Task<TResult>> func) => 
        new StructuredTask<TResult>(func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11));

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, StructuredTask<TResult>> func)
    {
        // This works because the structuredTask is assigned befor the await is hit.
        StructuredTask<TResult> structuredTask = default!;
        var impl = async () =>
        {
            structuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11);
            return await structuredTask;
        };

        return new StructuredTask<TResult>(impl(), structuredTask.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

     public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, StructuredTask<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, StructuredTask<TResult>> func)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, Task<TResult>> func) => 
        new StructuredTask<TResult>(func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12));

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, StructuredTask<TResult>> func)
    {
        // This works because the structuredTask is assigned befor the await is hit.
        StructuredTask<TResult> structuredTask = default!;
        var impl = async () =>
        {
            structuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12);
            return await structuredTask;
        };

        return new StructuredTask<TResult>(impl(), structuredTask.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

     public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, StructuredTask<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, StructuredTask<TResult>> func)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, Task<TResult>> func) => 
        new StructuredTask<TResult>(func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13));

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, StructuredTask<TResult>> func)
    {
        // This works because the structuredTask is assigned befor the await is hit.
        StructuredTask<TResult> structuredTask = default!;
        var impl = async () =>
        {
            structuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13);
            return await structuredTask;
        };

        return new StructuredTask<TResult>(impl(), structuredTask.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13);
        };

        return new StructuredTask<TResult>(impl());
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

     public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this Task<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, StructuredTask<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }

    public static StructuredTask<TResult> I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this StructuredTask<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13)> s, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, StructuredTask<TResult>> func)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13) source;
                try
                {
                    source = await s;
                }
                catch (OperationCanceledException)
                {
                    // If *source* was cancelled, cancel *our* task.
                    cts.Cancel(); // Ensure consistent cancellation.
                    tcs.SetCanceled(cts.Token); // Or SetCanceled() if you don't need the token
                    return;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    return;
                }

                var innerStructuredTask = func(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13);

                try
                {
                    using var innerRegistration = cts.Token.Register(() => innerStructuredTask.CancellationTokenSource.Cancel());
                    var result = await innerStructuredTask;
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
                // Catch-all: This should rarely happen, but protects against unexpected errors in the setup.
                tcs.TrySetException(ex);  // Use TrySetException, as the task might already be completed.
            }
        };
        impl();

        return new StructuredTask<TResult>(tcs.Task, cts);
    }
}
