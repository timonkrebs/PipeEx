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

    // The middle stage is gated on a TaskCompletionSource released only after Cancel(), so a
    // cancellation checkpoint always observes the cancel — a fixed Task.Delay would race the test
    // thread's Cancel() call and could complete the chain first on a slow scheduler. Asserts the
    // semantic contract (canceled, surfacing an OperationCanceledException) rather than the exact
    // exception subtype, which depends on which stage's checkpoint observes the cancellation first.
    [Fact]
    public Task Test8_Ensure_CancellationTokenSource_Cancellation() =>
        Arrange(() => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously))
        .Act(gate =>
        {
            var structuredTask = 1.I<int, int>(val => Task.FromResult(val * 2))
            .I(async x =>
            {
                await gate.Task;
                return x + 1;
            })
            .I(x => x + 1);

            structuredTask.CancellationTokenSource.Cancel();
            gate.SetResult();
            return structuredTask;
        })
        .Assert(async structuredTask =>
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await structuredTask);
            Assert.True(((Task<int>)structuredTask).IsCanceled);
        });

    // The wrapper overloads (Task -> StructuredTask and StructuredTask -> StructuredTask) must observe source
    // cancellation whether the source is already canceled BEFORE chaining (Test9/Test10) or cancels LATER
    // while being awaited (Test11/Test12). Both cases must complete the wrapper as a canceled task rather
    // than a faulted one, so each test also asserts IsCanceled (awaiting alone cannot distinguish a Canceled
    // task from one faulted with a TaskCanceledException).

    [Fact]
    public Task Test9_TaskSourceToStructuredTaskFunc_SourceCanceledBeforeChaining_CompletesAsCanceledTask() =>
        Arrange(() =>
        {
            var source = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            source.SetCanceled();
            return source;
        })
        .Act(x => x.Task.I(StructuredDouble))
        .Assert(async structuredTask =>
        {
            await Assert.ThrowsAsync<TaskCanceledException>(async () => await structuredTask);
            Assert.True(((Task<int>)structuredTask).IsCanceled);
        });

    [Fact]
    public Task Test10_StructuredTaskToStructuredTaskFunc_SourceCanceledBeforeChaining_CompletesAsCanceledTask() =>
        Arrange(() =>
        {
            var source = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            source.SetCanceled();
            return source;
        })
        .Act(x => x.Task.I(val => Task.FromResult(val * 2)).I(StructuredDouble))
        .Assert(async structuredTask =>
        {
            await Assert.ThrowsAsync<TaskCanceledException>(async () => await structuredTask);
            Assert.True(((Task<int>)structuredTask).IsCanceled);
        });

    [Fact]
    public Task Test11_TaskSourceToStructuredTaskFunc_SourceCanceledWhileAwaiting_CompletesAsCanceledTask() =>
        Arrange(() => new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously))
        .Act(x =>
        {
            var structuredTask = x.Task.I(StructuredDouble);

            x.SetCanceled();
            return structuredTask;
        })
        .Assert(async structuredTask =>
        {
            await Assert.ThrowsAsync<TaskCanceledException>(async () => await structuredTask);
            Assert.True(((Task<int>)structuredTask).IsCanceled);
        });

    [Fact]
    public Task Test12_StructuredTaskToStructuredTaskFunc_SourceCanceledWhileAwaiting_CompletesAsCanceledTask() =>
        Arrange(() => new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously))
        .Act(x =>
        {
            var structuredTask = x.Task.I(val => Task.FromResult(val * 2)).I(StructuredDouble);

            x.SetCanceled();
            return structuredTask;
        })
        .Assert(async structuredTask =>
        {
            await Assert.ThrowsAsync<TaskCanceledException>(async () => await structuredTask);
            Assert.True(((Task<int>)structuredTask).IsCanceled);
        });

    private static StructuredTask<int> StructuredDouble(int v) => v.I<int, int>(w => Task.FromResult(w * 2));
}