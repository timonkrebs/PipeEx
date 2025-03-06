#!/bin/bash
fileName="PipeEx.StructuredConcurrency/StructuredConcurrency.TupleDestructuring.g.cs"
echo "Started generating $fileName"
cat << EOF > $fileName
namespace PipeEx.StructuredConcurrency;

public static class TupleDestructuring
{
EOF
for item in $(seq 1 12); do
    ty="TSource1"
    tv="source.Item1"

    for i in $(seq 1 $item); do
        ty="${ty}, TSource$((i + 1))"
        tv="${tv}, source.Item$((i + 1))"
    done

cat << EOF >> $fileName

    public static async StructuredTask<TResult> I<$ty, TResult>(this ($ty) source, Func<$ty, Task<TResult>> func)
    {
        return await func($tv);
    }

    public static StructuredTask<TResult> I<$ty, TResult>(this ($ty) source, Func<$ty, StructuredTask<TResult>> func)
    {
        // This works because the structuredTask is assigned befor the await is hit.
        StructuredTask<TResult> structuredTask = default!;
        var impl = async () =>
        {
            structuredTask = func($tv);
            return await structuredTask;
        };

        return new StructuredTask<TResult>(impl(), structuredTask.CancellationTokenSource);
    }

    public static async StructuredTask<TResult> I<$ty, TResult>(this Task<($ty)> s, Func<$ty, TResult> func)
    {
        var source = await s;
        return func($tv);
    }

    public static StructuredTask<TResult> I<$ty, TResult>(this StructuredTask<($ty)> s, Func<$ty, TResult> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return func($tv);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

    public static async StructuredTask<TResult> I<$ty, TResult>(this Task<($ty)> s, Func<$ty, Task<TResult>> func)
    {
        var source = await s;
        return await func($tv);
    }

    public static StructuredTask<TResult> I<$ty, TResult>(this StructuredTask<($ty)> s, Func<$ty, Task<TResult>> func)
    {
        var impl = async () =>
        {
            var source = await s;
            return await func($tv);
        };

        return new StructuredTask<TResult>(impl(), s.CancellationTokenSource);
    }

     public static StructuredTask<TResult> I<$ty, TResult>(this Task<($ty)> s, Func<$ty, StructuredTask<TResult>> func)
    {
        var cts = new CancellationTokenSource();
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                ($ty) source;
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

                var innerStructuredTask = func($tv);

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

    public static StructuredTask<TResult> I<$ty, TResult>(this StructuredTask<($ty)> s, Func<$ty, StructuredTask<TResult>> func)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(s.CancellationTokenSource.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        var impl = async () =>
        {
            try
            {
                ($ty) source;
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

                var innerStructuredTask = func($tv);

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
EOF

done
cat << EOF >> $fileName
}
EOF
echo "Successfully generated $fileName"