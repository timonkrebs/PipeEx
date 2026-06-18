using PipeEx.StructuredConcurrency;
using static BunsenBurner.ArrangeActAssert;

namespace PipeEx.Tests;

// Characterization coverage for the StructuredTask chaining wrappers. The existing suite exercises
// these paths only for cancellation (StructuredConcurrencyTests Test9-12) or not at all; these tests
// pin success and fault-propagation behavior ahead of unifying the chaining core, including the
// brand-new tuple -> StructuredTask helper (ChainTupleToStructured).
public class StructuredConcurrencyChainingTests
{
    [Fact]
    public Task StructuredTaskSource_SyncFunc_Success() =>
        Arrange(() => new StructuredTask<int>(Task.FromResult(5), CancellationToken.None))
        .Act(s => s.I(v => v + 1))
        .Assert(async r => Assert.Equal(6, await r));

    [Fact]
    public Task StructuredTaskSource_AsyncFunc_Success() =>
        Arrange(() => new StructuredTask<int>(Task.FromResult(5), CancellationToken.None))
        .Act(s => s.I(v => Task.FromResult(v + 1)))
        .Assert(async r => Assert.Equal(6, await r));

    [Fact]
    public Task TaskSource_StructuredTaskFunc_Success() =>
        Arrange(() => Task.FromResult(5))
        .Act(s => s.I(StructuredDouble))
        .Assert(async r => Assert.Equal(10, await r));

    [Fact]
    public Task StructuredTaskSource_StructuredTaskFunc_Success() =>
        Arrange(() => new StructuredTask<int>(Task.FromResult(5), CancellationToken.None))
        .Act(s => s.I(StructuredDouble))
        .Assert(async r => Assert.Equal(10, await r));

    [Fact]
    public Task TupleTaskSource_StructuredTaskFunc_Success() =>
        Arrange(() => Task.FromResult((2, 3)))
        .Act(s => s.I<int, int, int>((a, b) => StructuredDouble(a + b)))
        .Assert(async r => Assert.Equal(10, await r));

    [Fact]
    public Task TupleStructuredTaskSource_StructuredTaskFunc_Success() =>
        Arrange(() => new StructuredTask<(int, int)>(Task.FromResult((2, 3)), CancellationToken.None))
        .Act(s => s.I<int, int, int>((a, b) => StructuredDouble(a + b)))
        .Assert(async r => Assert.Equal(10, await r));

    [Fact]
    public Task StructuredTaskSource_AsyncFunc_InnerFaultPropagates() =>
        Arrange(() => new StructuredTask<int>(Task.FromResult(5), CancellationToken.None))
        .Act(s => s.I<int, int>(async v => { await Task.Yield(); throw new InvalidOperationException("boom"); }))
        .Assert(async r =>
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await r);
            Assert.Equal("boom", ex.Message);
        });

    [Fact]
    public Task TaskSource_StructuredTaskFunc_InnerFaultPropagates() =>
        Arrange(() => Task.FromResult(5))
        .Act(s => s.I(StructuredThrow))
        .Assert(async r =>
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await r);
            Assert.Equal("boom", ex.Message);
        });

    [Fact]
    public Task LetAwait_DeferredFaultPropagates() =>
        Arrange(() => 1)
        .Act(x => x.Let(() => ThrowAsync()).Await((s, d) => s + d))
        .Assert(async r => await Assert.ThrowsAsync<InvalidOperationException>(async () => await r));

    [Fact]
    public Task Let_StructuredTaskSource_AsyncFunc_Success() =>
        Arrange(() => new StructuredTask<int>(Task.FromResult(5), CancellationToken.None))
        .Act(s => s.Let(v => Task.FromResult(v * 2)).Await((src, d) => src + d))
        .Assert(async r => Assert.Equal(15, await r));

    // Regression: a source-arg Let chained onto a deferred task must keep the earlier deferred.
    // It previously bound to the StructuredTask-source Let (via OverloadResolutionPriority) and dropped
    // it; with the deferred-source Let prioritised the 3-deferred result is preserved (1 + 10 + 101).
    [Fact]
    public Task Let_DeferredChain_SourceArgFunc_PreservesIntermediateDeferred() =>
        Arrange(() => 1)
        .Act(x => x.Let(() => Task.FromResult(10)).Let(v => Task.FromResult(v + 100)).Await((src, a, b) => src + a + b))
        .Assert(async r => Assert.Equal(112, await r));

    // A deferred Let must thread the source's CancellationTokenSource into the extended chain (rather
    // than a fresh one) so that cancelling the final task reaches the newly added deferred stage.
    [Fact]
    public void DeferredLet_SharesSourceCancellationTokenSource()
    {
        var source = 1.Let(() => Task.FromResult(10));
        var chained = source.Let(v => Task.FromResult(v + 100));
        Assert.Same(source.CancellationTokenSource, chained.CancellationTokenSource);
    }

    // Cancellation of the final chain must reach a newly added deferred stage: with the source still
    // pending, cancelling and then completing the source throws and the deferred work never starts.
    [Fact]
    public async Task DeferredLet_CancellationReachesDeferredStage()
    {
        var deferredStarted = false;
        var sourceTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        var chain = sourceTcs.Task.I(v => v)
            .Let(v => { deferredStarted = true; return Task.FromResult(v + 1); })
            .Await((s, d) => s + d);

        chain.CancellationTokenSource.Cancel();
        sourceTcs.SetResult(5);

        await Assert.ThrowsAsync<OperationCanceledException>(async () => await chain);
        Assert.False(deferredStarted);
    }

    // Await observes every deferred result even when the projection discards one, so a fault in a
    // discarded deferred still surfaces.
    [Fact]
    public Task LetAwait_ObservesFaultInDiscardedDeferred() =>
        Arrange(() => 1)
        .Act(x => x.Let(() => Task.FromResult(10)).Let(() => ThrowAsync()).Await((s, a, _) => s + a))
        .Assert(async r => await Assert.ThrowsAsync<InvalidOperationException>(async () => await r));

    private static StructuredTask<int> StructuredDouble(int v) => v.I<int, int>(w => Task.FromResult(w * 2));
    private static StructuredTask<int> StructuredThrow(int v) => v.I<int, int>(async _ => { await Task.Yield(); throw new InvalidOperationException("boom"); });
    private static async Task<int> ThrowAsync() { await Task.Yield(); throw new InvalidOperationException("deferred boom"); }
}
