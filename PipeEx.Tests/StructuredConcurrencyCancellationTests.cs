using PipeEx.StructuredConcurrency;
using static BunsenBurner.ArrangeActAssert;

namespace PipeEx.Tests;

// Coverage for the "deeper cancellation" feature: the cancellation-aware I / Let overloads flow the
// pipe's carried CancellationToken into each stage's job, so a running operation can be interrupted in
// flight rather than only observed between stages.
//
// The interruption tests block the job on Task.Delay(Timeout.Infinite, ct): an infinite delay can only
// complete via the token it was handed, so if the chain completes at all the flowed-in token must have
// reached the running work. AwaitCanceledPromptly fails fast (instead of hanging the suite) if it does
// not.
public class StructuredConcurrencyCancellationTests
{
    // --- Success: a token-aware job that is never cancelled runs to completion and its value flows ----

    [Fact]
    public Task I_ValueSource_TokenJob_Success() =>
        Arrange(() => 5)
        .Act(x => x.I(async (int v, CancellationToken ct) => { await Task.Yield(); return v * 2; }))
        .Assert(async r => Assert.Equal(10, await r));

    [Fact]
    public Task I_TaskSource_TokenJob_Success() =>
        Arrange(() => Task.FromResult(5))
        .Act(x => x.I(async (int v, CancellationToken ct) => { await Task.Yield(); return v * 2; }))
        .Assert(async r => Assert.Equal(10, await r));

    [Fact]
    public Task I_StructuredTaskSource_TokenJob_Success() =>
        Arrange(() => new StructuredTask<int>(Task.FromResult(5), CancellationToken.None))
        .Act(s => s.I(async (int v, CancellationToken ct) => { await Task.Yield(); return v * 2; }))
        .Assert(async r => Assert.Equal(10, await r));

    [Fact]
    public Task Let_ValueSource_TokenDeferred_Success() =>
        Arrange(() => 2)
        .Act(x => x.Let(async (int v, CancellationToken ct) => { await Task.Yield(); return v * 5; })
                   .Await((src, d) => src + d))
        .Assert(async r => Assert.Equal(12, await r));

    [Fact]
    public Task Let_StructuredTaskSource_TokenDeferred_Success() =>
        Arrange(() => new StructuredTask<int>(Task.FromResult(3), CancellationToken.None))
        .Act(s => s.Let(async (int v, CancellationToken ct) => { await Task.Yield(); return v * 4; })
                   .Await((src, d) => src * d))
        .Assert(async r => Assert.Equal(36, await r));

    // Two token-aware Lets must resolve to the deferred-source overload for the second one (priority 2,
    // matched via inheritance) so both deferreds are preserved rather than one being dropped: 1 + 11 + 101.
    [Fact]
    public Task Let_TwoTokenDeferreds_Await3_PreservesBothDeferreds() =>
        Arrange(() => 1)
        .Act(x => x.Let((int v, CancellationToken ct) => Task.FromResult(v + 10))
                   .Let((int v, CancellationToken ct) => Task.FromResult(v + 100))
                   .Await((src, a, b) => src + a + b))
        .Assert(async r => Assert.Equal(113, await r));

    // The flowed-in token is the chain's own, so a job/deferred that ignores it but completes normally
    // still produces a value with the chain left uncancelled.
    [Fact]
    public async Task I_TokenJob_RunsWithChainsOwnToken()
    {
        CancellationToken seen = default;
        var chain = 5.I(async (int v, CancellationToken ct) => { await Task.Yield(); seen = ct; return v; });
        await chain;
        Assert.Equal(chain.CancellationTokenSource.Token, seen);
    }

    // --- In-flight interruption: cancelling the chain stops work blocked inside an await ---------------

    [Fact]
    public Task I_ValueSource_TokenJob_CancellingChainInterruptsInFlightWork()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var chain = 5.I(async (int v, CancellationToken ct) =>
        {
            started.SetResult();
            await Task.Delay(Timeout.Infinite, ct);
            return v;
        });

        return Cancel(chain, started);
    }

    [Fact]
    public Task I_TaskSource_TokenJob_CancellingChainInterruptsInFlightWork()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var chain = Task.FromResult(5).I(async (int v, CancellationToken ct) =>
        {
            started.SetResult();
            await Task.Delay(Timeout.Infinite, ct);
            return v;
        });

        return Cancel(chain, started);
    }

    [Fact]
    public Task I_StructuredTaskSource_TokenJob_CancellingChainInterruptsInFlightWork()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var source = new StructuredTask<int>(Task.FromResult(5), CancellationToken.None);
        var chain = source.I(async (int v, CancellationToken ct) =>
        {
            started.SetResult();
            await Task.Delay(Timeout.Infinite, ct);
            return v;
        });

        return Cancel(chain, started);
    }

    // The headline scenario: an earlier stage completes, a later stage is in flight, and cancelling the
    // chain reaches into that later stage (the token is shared along the chain).
    [Fact]
    public async Task I_TokenJob_CancellationInterruptsInFlightLaterStage()
    {
        var reachedStage2 = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var source = new StructuredTask<int>(Task.FromResult(1), CancellationToken.None);

        var chain = source
            .I(async (int v, CancellationToken ct) => { await Task.Yield(); return v + 1; })
            .I(async (int v, CancellationToken ct) =>
            {
                reachedStage2.SetResult();
                await Task.Delay(Timeout.Infinite, ct);
                return v;
            });

        await reachedStage2.Task;
        chain.CancellationTokenSource.Cancel();
        await AwaitCanceledPromptly(chain);
    }

    [Fact]
    public async Task Let_ValueSource_TokenDeferred_CancellingChainInterruptsInFlightWork()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var deferred = 5.Let(async (int v, CancellationToken ct) =>
        {
            started.SetResult();
            await Task.Delay(Timeout.Infinite, ct);
            return v;
        });
        var joined = deferred.Await((s, d) => s + d);

        await started.Task;
        deferred.CancellationTokenSource.Cancel();
        await AwaitCanceledPromptly(joined);
    }

    [Fact]
    public async Task Let_StructuredTaskSource_TokenDeferred_CancellingChainInterruptsInFlightWork()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var source = new StructuredTask<int>(Task.FromResult(5), CancellationToken.None);
        var deferred = source.Let(async (int v, CancellationToken ct) =>
        {
            started.SetResult();
            await Task.Delay(Timeout.Infinite, ct);
            return v;
        });
        var joined = deferred.Await((s, d) => s + d);

        await started.Task;
        deferred.CancellationTokenSource.Cancel();
        await AwaitCanceledPromptly(joined);
    }

    // --- Wiring: shared token, up-front check, disposal, coexistence ----------------------------------

    // The cancellation-aware stages share one CancellationTokenSource along the chain, so a single cancel
    // reaches whichever stage is running.
    [Fact]
    public async Task I_TokenJobs_ShareOneCancellationTokenSourceAcrossStages()
    {
        var source = new StructuredTask<int>(Task.FromResult(1), CancellationToken.None);
        var stage2 = source
            .I(async (int v, CancellationToken ct) => { await Task.Yield(); return v + 1; })
            .I(async (int v, CancellationToken ct) => { await Task.Yield(); return v + 1; });

        Assert.Same(source.CancellationTokenSource, stage2.CancellationTokenSource);
        Assert.Equal(3, await stage2);
    }

    // The token-aware StructuredTask-source overload runs its up-front cancellation check synchronously,
    // so an already-cancelled source throws from the I(...) call itself (matching the token-free path).
    [Fact]
    public void I_StructuredTaskSource_TokenJob_SourceCtsCanceledBeforeChaining_ThrowsAtCallSite()
    {
        var source = new StructuredTask<int>(Task.FromResult(5), CancellationToken.None);
        source.CancellationTokenSource.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(
            () => { _ = source.I((int v, CancellationToken ct) => Task.FromResult(v + 1)); });
    }

    // The value/Task entry points own the fresh CancellationTokenSource they create and dispose it with
    // the resulting StructuredTask.
    [Fact]
    public async Task I_ValueSource_TokenJob_OwnsAndDisposesCtsAfterAwait()
    {
        var chain = 5.I(async (int v, CancellationToken ct) => { await Task.Yield(); return v * 2; });
        using (chain) { Assert.Equal(10, await chain); }
        Assert.Throws<ObjectDisposedException>(() => chain.CancellationTokenSource.Token.ThrowIfCancellationRequested());
    }

    // Cancellation-aware and token-free stages compose in one chain; the token-free stage simply does not
    // observe the token, while the chain as a whole still produces its value.
    [Fact]
    public async Task I_TokenAndTokenFreeStages_Compose()
    {
        var source = new StructuredTask<int>(Task.FromResult(1), CancellationToken.None);
        var chain = source
            .I(async (int v, CancellationToken ct) => { await Task.Yield(); return v + 1; })
            .I(v => Task.FromResult(v * 10))
            .I(async (int v, CancellationToken ct) => { await Task.Yield(); return v + 3; });

        Assert.Equal(23, await chain);
    }

    // A cancellation-aware value-source Let factory that throws synchronously surfaces at the call site,
    // matching the token-free value-source Let (the overload disposes the token source it created).
    [Fact]
    public void Let_ValueSource_TokenFactoryThrowsSync_ThrowsAtCallSite()
    {
        Func<int, CancellationToken, Task<int>> faulting = (_, _) => throw new InvalidOperationException("sync boom");
        var ex = Assert.Throws<InvalidOperationException>(() => { _ = 5.Let(faulting); });
        Assert.Equal("sync boom", ex.Message);
    }

    // --- Tuple sources: the same token flows through the generated tuple I overloads ----------------

    [Fact]
    public Task I_ValueTupleSource_TokenJob_Success() =>
        Arrange(() => (2, 3))
        .Act(t => t.I(async (int a, int b, CancellationToken ct) => { await Task.Yield(); return a + b; }))
        .Assert(async r => Assert.Equal(5, await r));

    [Fact]
    public Task I_TaskTupleSource_TokenJob_Success() =>
        Arrange(() => Task.FromResult((2, 3)))
        .Act(t => t.I(async (int a, int b, CancellationToken ct) => { await Task.Yield(); return a * b; }))
        .Assert(async r => Assert.Equal(6, await r));

    [Fact]
    public async Task I_StructuredTaskTupleSource_TokenJob_CancellingChainInterruptsInFlightWork()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var source = new StructuredTask<(int, int)>(Task.FromResult((2, 3)), CancellationToken.None);
        var chain = source.I(async (int a, int b, CancellationToken ct) =>
        {
            started.SetResult();
            await Task.Delay(Timeout.Infinite, ct);
            return a + b;
        });

        await started.Task;
        chain.CancellationTokenSource.Cancel();
        await AwaitCanceledPromptly(chain);
    }

    // The generated tuple overloads route every await through the same CheckedAwait path as the scalar
    // overloads, so cancellation is honoured even when the tuple job ignores the token it was handed.

    // Cancellation requested before the source completes: the checked await throws after the source, so
    // the job is never invoked (without the checked path the job would run and the chain would complete
    // successfully despite Cancel()).
    [Fact]
    public async Task I_TaskTupleSource_TokenIgnoringJob_CancelBeforeSource_SkipsJobAndCancels()
    {
        var sourceTcs = new TaskCompletionSource<(int, int)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var jobInvoked = false;
        var chain = sourceTcs.Task.I((int a, int b, CancellationToken ct) =>
        {
            jobInvoked = true;
            return Task.FromResult(a + b);
        });

        chain.CancellationTokenSource.Cancel();
        sourceTcs.SetResult((2, 3));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await chain);
        Assert.False(jobInvoked);
    }

    // Cancellation requested while a token-ignoring job runs and then returns normally: the trailing
    // checked await surfaces the cancellation after the job completes.
    [Fact]
    public async Task I_ValueTupleSource_TokenIgnoringJob_CancelDuringRun_Cancels()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var chain = (2, 3).I(async (int a, int b, CancellationToken ct) =>
        {
            started.SetResult();
            await release.Task;          // ignores ct
            return a + b;                // returns normally
        });

        await started.Task;
        chain.CancellationTokenSource.Cancel();   // requested while the job ran and ignored it
        release.SetResult();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await chain);
    }

    // The token-aware StructuredTask tuple overload runs its up-front cancellation check synchronously,
    // so an already-cancelled source throws from the I(...) call itself (matching the scalar overload).
    [Fact]
    public void I_StructuredTaskTupleSource_SourceCtsCanceledBeforeChaining_ThrowsAtCallSite()
    {
        var source = new StructuredTask<(int, int)>(Task.FromResult((2, 3)), CancellationToken.None);
        source.CancellationTokenSource.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(
            () => { _ = source.I((int a, int b, CancellationToken ct) => Task.FromResult(a + b)); });
    }

    // Cancels the chain once its job is in flight and asserts the job was interrupted (not run to
    // completion or left hanging).
    private static async Task Cancel(StructuredTask<int> chain, TaskCompletionSource started)
    {
        await started.Task;
        chain.CancellationTokenSource.Cancel();
        await AwaitCanceledPromptly(chain);
    }

    private static async Task AwaitCanceledPromptly(StructuredTask<int> chain)
    {
        var task = (Task<int>)chain;
        var finished = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(10)));
        Assert.True(finished == task, "the in-flight job was not interrupted by the flowed-in cancellation token");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        Assert.True(task.IsCanceled);
    }
}
