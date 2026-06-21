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
    public Task Test6_Ensure_Cancellation_Chaining_ChainedAfterCancel() =>
        Arrange(() => new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously))
        .Act(x =>
        {
            var structuredTask = x.Task.I(val => Task.FromResult(val * 2));

            x.SetCanceled();
            return structuredTask.I(val => val * 2);
        })
        .Assert(async structuredTask => await Assert.ThrowsAsync<TaskCanceledException>(async () => await structuredTask));

    [Fact]
    public Task Test7_Ensure_CancellationTokenSource_Cancellation() =>
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

    // The wrapper overloads (Task -> StructuredTask and StructuredTask -> StructuredTask) must observe source
    // cancellation whether the source is already canceled BEFORE chaining (Test8/Test9) or cancels LATER
    // while being awaited (Test10/Test11). Both cases must complete the wrapper as a canceled task rather
    // than a faulted one, so each test also asserts IsCanceled (awaiting alone cannot distinguish a Canceled
    // task from one faulted with a TaskCanceledException).

    [Fact]
    public Task Test8_TaskSourceToStructuredTaskFunc_SourceCanceledBeforeChaining_CompletesAsCanceledTask() =>
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
    public Task Test9_StructuredTaskToStructuredTaskFunc_SourceCanceledBeforeChaining_CompletesAsCanceledTask() =>
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
    public Task Test10_TaskSourceToStructuredTaskFunc_SourceCanceledWhileAwaiting_CompletesAsCanceledTask() =>
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
    public Task Test11_StructuredTaskToStructuredTaskFunc_SourceCanceledWhileAwaiting_CompletesAsCanceledTask() =>
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
