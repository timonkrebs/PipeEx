using PipeEx.StructuredConcurrency;

namespace PipeEx.Tests;

// Characterization coverage for the StructuredTask-source I overloads (the CheckedChain path) and the
// Await projections: cancellation (raised up-front at the call site and mid-flight while awaiting),
// fault propagation, and CancellationTokenSource ownership/disposal. The existing suites pin these for
// the -> StructuredTask paths (StructuredConcurrencyTests Test9-12) and for success (AsyncLetTests);
// these lock the remaining cancellation/fault/disposal contract ahead of unifying the two chaining
// cores (CheckedChain + ChainInnerStructured) and collapsing the repeated ThrowIfCancellationRequested.
public class StructuredConcurrencyCoreTests
{
    // --- StructuredTask-source I(-> T) / I(-> Task): the CheckedChain path ----------------------

    // The up-front cancellation check runs synchronously inside the chaining call, so an already-cancelled
    // source CTS throws from the I(...) call itself rather than producing a task that later cancels.
    [Fact]
    public void I_StructuredTaskSource_SyncFunc_SourceCtsCanceledBeforeChaining_ThrowsAtCallSite()
    {
        var source = new StructuredTask<int>(Task.FromResult(5), CancellationToken.None);
        source.CancellationTokenSource.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(() => { _ = source.I(v => v + 1); });
    }

    [Fact]
    public void I_StructuredTaskSource_AsyncFunc_SourceCtsCanceledBeforeChaining_ThrowsAtCallSite()
    {
        var source = new StructuredTask<int>(Task.FromResult(5), CancellationToken.None);
        source.CancellationTokenSource.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(() => { _ = source.I(v => Task.FromResult(v + 1)); });
    }

    // A sync projection that throws faults the resulting task (typed Func avoids overload ambiguity that
    // a throwing lambda would introduce between the -> T / -> Task / -> StructuredTask I overloads).
    [Fact]
    public async Task I_StructuredTaskSource_SyncFunc_FuncThrows_FaultsTask()
    {
        var source = new StructuredTask<int>(Task.FromResult(5), CancellationToken.None);
        Func<int, int> faulting = _ => throw new InvalidOperationException("boom");
        var structuredTask = source.I(faulting);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await structuredTask);
        Assert.Equal("boom", ex.Message);
    }

    [Fact]
    public async Task I_StructuredTaskSource_AsyncFunc_FuncThrows_FaultsTask()
    {
        var source = new StructuredTask<int>(Task.FromResult(5), CancellationToken.None);
        Func<int, Task<int>> faulting = async _ => { await Task.Yield(); throw new InvalidOperationException("boom"); };
        var structuredTask = source.I(faulting);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await structuredTask);
        Assert.Equal("boom", ex.Message);
    }

    // The StructuredTask-source chain shares the source's CancellationTokenSource (ownership transfer) and
    // disposing the result disposes that CTS.
    [Fact]
    public async Task I_StructuredTaskSource_SharesCtsAndDisposesAfterAwait()
    {
        var source = new StructuredTask<int>(Task.FromResult(5), CancellationToken.None);
        var result = source.I(v => v + 1);
        Assert.Same(source.CancellationTokenSource, result.CancellationTokenSource);

        using (result) { Assert.Equal(6, await result); }

        Assert.Throws<ObjectDisposedException>(() => result.CancellationTokenSource.Token.ThrowIfCancellationRequested());
    }

    // --- -> StructuredTask path (ChainInnerStructured): disposal and fault ----------------------

    [Fact]
    public async Task I_TaskSource_StructuredTaskFunc_DisposesCtsAfterAwait()
    {
        var structuredTask = Task.FromResult(5).I(StructuredDouble);
        using (structuredTask) { Assert.Equal(10, await structuredTask); }
        Assert.Throws<ObjectDisposedException>(() => structuredTask.CancellationTokenSource.Token.ThrowIfCancellationRequested());
    }

    // An inner fault propagates but does not cancel the chain's CancellationTokenSource (a fault is not a
    // cancellation). Pins the ChainInnerStructured fault path that unification must preserve.
    [Fact]
    public async Task I_TaskSource_StructuredTaskFunc_InnerFault_DoesNotCancelCts()
    {
        var structuredTask = Task.FromResult(5).I(StructuredThrow);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await structuredTask);
        Assert.False(structuredTask.CancellationTokenSource.IsCancellationRequested);
    }

    // --- Await: cancellation and fault propagation ----------------------------------------------

    // Await's up-front cancellation check runs synchronously, so an already-cancelled chain throws from
    // the Await(...) call itself.
    [Fact]
    public void Await_SourceCtsCanceledBeforeAwait_ThrowsAtCallSite()
    {
        var deferred = 1.Let(() => Task.FromResult(10));
        deferred.CancellationTokenSource.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(() => { _ = deferred.Await((s, d) => s + d); });
    }

    // Cancelling while the source is still pending, then completing it, completes the Await as canceled.
    [Fact]
    public async Task Await_CancellationDuringSource_CompletesAsCanceled()
    {
        var sourceTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var deferred = new StructuredTask<int>(sourceTcs.Task, CancellationToken.None)
            .Let(v => Task.FromResult(v + 1))
            .Await((s, d) => s + d);

        deferred.CancellationTokenSource.Cancel();
        sourceTcs.SetResult(5);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await deferred);
    }

    [Fact]
    public async Task Await_SourceFaults_PropagatesThroughAwait()
    {
        var sourceTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var deferred = new StructuredTask<int>(sourceTcs.Task, CancellationToken.None)
            .Let(v => Task.FromResult(v + 1))
            .Await((s, d) => s + d);

        sourceTcs.SetException(new InvalidOperationException("boom"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await deferred);
        Assert.Equal("boom", ex.Message);
    }

    [Fact]
    public async Task Await_ProjectionThrows_FaultsResult()
    {
        Func<int, int, int> projection = (s, d) => throw new InvalidOperationException("boom");
        var deferred = 1.Let(() => Task.FromResult(10)).Await(projection);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await deferred);
        Assert.Equal("boom", ex.Message);
    }

    private static StructuredTask<int> StructuredDouble(int v) => v.I<int, int>(w => Task.FromResult(w * 2));
    private static StructuredTask<int> StructuredThrow(int v) => v.I<int, int>(async _ => { await Task.Yield(); throw new InvalidOperationException("boom"); });
}
