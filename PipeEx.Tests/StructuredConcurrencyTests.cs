using PipeEx.StructuredConcurrency;
using static BunsenBurner.ArrangeActAssert;

namespace PipeEx.Tests;

public class StructuredConcurrencyTests
{
    [Fact]
    public Task Test1_InnerTaskExceptionAsync() =>
        Arrange(() => 1)
        .Act(x => x.I<int, int>(async _ => { await Task.Yield(); throw new InvalidOperationException("Inner failed"); }))
        .Assert(async r =>
        {
            // Assert that the resulting task propagates the exception.
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await r);
            Assert.Equal("Inner failed", ex.Message);
        });

    [Fact]
    public Task Test2_Ensure_Cancellation_Registration_Is_Disposed() =>
        Arrange(() => new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously))
        .Act(x =>
        {
            var structuredTask = x.Task.I(val => Task.FromResult(val * 2));
            // We don't cancel
            x.SetResult(5);  // Complete the source task to cause disposal.
            return structuredTask;
        })
        .Assert(async structuredTask =>
        {
            using (var s = structuredTask)
            {
                await structuredTask;
            }

            // Check if the CancellationToken is disposed, it throws an exception when used after disposal
            Assert.Throws<ObjectDisposedException>(() => structuredTask.CancellationTokenSource.Token.ThrowIfCancellationRequested());
        });

    [Fact]
    public Task Test3_Ensure_Cancellation_Registration_Is_Disposed() =>
        Arrange(() => new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously))
        .Act(x =>
        {
            var structuredTask = x.Task.I(val => Task.FromResult(val * 2))
                .I(val => Task.FromResult(val * 2))
                .I(val => Task.FromResult(val * 2));

            // We don't cancel
            x.SetResult(5);  // Complete the source task to cause disposal.
            return structuredTask;
        })
        .Assert(async structuredTask =>
        {
            using (var s = structuredTask)
            {
                await structuredTask;
            }

            // Check if the CancellationToken is disposed, it throws an exception when used after disposal
            Assert.Throws<ObjectDisposedException>(() => structuredTask.CancellationTokenSource.Token.ThrowIfCancellationRequested());
        });

    [Fact]
    public Task Test4_Ensure_Cancellation() =>
        Arrange(() => new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously))
        .Act(x =>
        {
            var structuredTask = x.Task.I(val => Task.FromResult(val * 2));

            x.SetCanceled();
            return structuredTask;
        })
        .Assert(async structuredTask => await Assert.ThrowsAsync<TaskCanceledException>(async () => await structuredTask));

    [Fact]
    public Task Test5_Ensure_Cancellation_Chaining_MultipleStages() =>
        Arrange(() => new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously))
        .Act(x =>
        {
            var structuredTask = x.Task.I(val => Task.FromResult(val * 2))
                                    .I(val => val * 2);

            x.SetCanceled();
            return structuredTask;
        })
        .Assert(async structuredTask => await Assert.ThrowsAsync<TaskCanceledException>(async () => await structuredTask));

    [Fact]
    public Task Test6_Ensure_Cancellation_Chaining_MultipleStages() =>
        Arrange(() => new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously))
        .Act(x =>
        {
            var structuredTask = x.Task.I(val => Task.FromResult(val * 2));

            x.SetCanceled();
            return structuredTask.I(val => val * 2);
        })
        .Assert(async structuredTask => await Assert.ThrowsAsync<TaskCanceledException>(async () => await structuredTask));

    [Fact]
    public Task Test7_Ensure_Cancellation_Chaining_MultipleStages() =>
        Arrange(() => new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously))
        .Act(x =>
        {
            var structuredTask = x.Task.I(val => Task.FromResult(val * 2));

            x.SetCanceled();
            return structuredTask.I(val => val * 2);
        })
        .Assert(async structuredTask => await Assert.ThrowsAsync<TaskCanceledException>(async () => await structuredTask));

    [Fact]
    public Task Test8_Ensure_CancellationTokenSource_Cancellation() =>
        Arrange(() => 1)
        .Act(x =>
        {
            var structuredTask = x.I<int, int>(val => Task.FromResult(val * 2))
            .I(async x =>
            {
                await Task.Delay(10);
                return x + 1;
            })
            .I(x => x + 1);

            structuredTask.CancellationTokenSource.Cancel();
            return structuredTask;
        })
        .Assert(async structuredTask => await Assert.ThrowsAsync<OperationCanceledException>(async () => await structuredTask));

    // Exercises the (value, Func<TSource, StructuredTask<TResult>>) overload. A method group is used
    // deliberately: a lambda returning a StructuredTask would bind to the higher-priority Task overload
    // (StructuredTask<T> is implicitly convertible to Task<T>), but a method group's return type is not
    // reference-convertible, so it resolves to the StructuredTask overload under test.
    [Fact]
    public Task Test9_ValueToStructuredTaskFunc_PropagatesResult() =>
        Arrange(() => 5)
        .Act(x => x.I(StructuredDouble))
        .Assert(async structuredTask => Assert.Equal(10, await structuredTask));

    [Fact]
    public Task Test10_ValueToStructuredTaskFunc_PropagatesException() =>
        Arrange(() => 1)
        .Act(x => x.I(StructuredThrow))
        .Assert(async structuredTask =>
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await structuredTask);
            Assert.Equal("boom", ex.Message);
        });

    [Fact]
    public Task Test11_TaskSourceToStructuredTaskFunc_SourceCancellation_CompletesAsCanceledTask() =>
        Arrange(() => new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously))
        .Act(x =>
        {
            var structuredTask = x.Task.I(StructuredDouble);

            x.SetCanceled();
            return structuredTask;
        })
        .Assert(async structuredTask => await Assert.ThrowsAsync<TaskCanceledException>(async () => await structuredTask));

    [Fact]
    public Task Test12_StructuredTaskToStructuredTaskFunc_SourceCancellation_CompletesAsCanceledTask() =>
        Arrange(() => new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously))
        .Act(x =>
        {
            var structuredTask = x.Task.I(val => Task.FromResult(val * 2)).I(StructuredDouble);

            x.SetCanceled();
            return structuredTask;
        })
        .Assert(async structuredTask => await Assert.ThrowsAsync<TaskCanceledException>(async () => await structuredTask));

    private static StructuredTask<int> StructuredDouble(int v) => v.I<int, int>(w => Task.FromResult(w * 2));

    private static StructuredTask<int> StructuredThrow(int v) =>
        v.I<int, int>(async _ => { await Task.Yield(); throw new InvalidOperationException("boom"); });
}