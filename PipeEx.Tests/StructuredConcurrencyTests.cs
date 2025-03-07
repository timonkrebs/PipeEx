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

    [Fact(Skip = "This test is temporarily disabled because Disposal of cancelation token is still in reaserch phase.")]
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
            await structuredTask;
            await Task.Yield();

            // Check if the CancellationToken is disposed, it throws an exception when used after disposal
            Assert.Throws<ObjectDisposedException>(() => structuredTask.CancellationTokenSource.Token.ThrowIfCancellationRequested());
        });

    [Fact]
    public Task Test3_Ensure_Cancellation() =>
        Arrange(() => new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously))
        .Act(x =>
        {
            var structuredTask = x.Task.I(val => Task.FromResult(val * 2));

            x.SetCanceled();
            return structuredTask;
        })
        .Assert(async structuredTask => await Assert.ThrowsAsync<TaskCanceledException>(async () => await structuredTask));

    [Fact]
    public Task Test4_Ensure_Cancellation_Chaining_MultipleStages() =>
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
    public Task Test5_Ensure_Cancellation_Chaining_MultipleStages() =>
        Arrange(() => new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously))
        .Act(x =>
        {
            var structuredTask = x.Task.I(val => Task.FromResult(val * 2));

            x.SetCanceled();
            return structuredTask.I(val => val * 2);
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
    public Task Test7_Ensure_CancellationTokenSource_Cancellation() =>
        Arrange(() => 1)
        .Act(x =>
        {
            var structuredTask = x.I(val => Task.FromResult(val * 2))
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
}