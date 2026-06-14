using PipeEx;
using PipeEx.StructuredConcurrency;
using static BunsenBurner.ArrangeActAssert;

// This namespace is intentionally NOT nested under PipeEx. It mirrors how a real consumer imports the
// library, so a method group returning StructuredTask<T> binds to the value->StructuredTask "wrap"
// overload (PipeEx.StructuredConcurrency) rather than the core synchronous pipe (PipeEx.Core.I).
//
// From a namespace nested under PipeEx (e.g. PipeEx.Tests) the core I is an enclosing-namespace
// extension and shadows the using-imported wrap overload whenever it is applicable, so the same method
// group would resolve to the core pipe instead. These tests therefore live here on purpose.
namespace StructuredConcurrencyConsumerTests;

public class WrapOverloadTests
{
    // ---------- overload resolution ----------

    [Fact]
    public void MethodGroup_BindsToWrapOverload_NotCorePipe()
    {
        StructuredTask<int> inner = null!;
        StructuredTask<int> Capture(int v) { inner = v.I<int, int>(w => Task.FromResult(w * 2)); return inner; }

        var wrapper = 5.I(Capture);

        // PipeEx.Core.I would return func(source) unchanged (same reference); the wrap overload allocates
        // a new handle that shares func's CancellationTokenSource.
        Assert.NotSame(inner, wrapper);
        Assert.Same(inner.CancellationTokenSource, wrapper.CancellationTokenSource);
    }

    [Fact]
    public void Tuple_MethodGroup_BindsToWrapOverload_NotCorePipe()
    {
        StructuredTask<int> inner = null!;
        StructuredTask<int> Capture(int a, int b) { inner = (a + b).I<int, int>(w => Task.FromResult(w)); return inner; }

        var wrapper = (2, 3).I(Capture);

        Assert.NotSame(inner, wrapper);
        Assert.Same(inner.CancellationTokenSource, wrapper.CancellationTokenSource);
    }

    // ---------- result / exception propagation ----------

    [Fact]
    public Task PropagatesResult() =>
        Arrange(() => 5)
        .Act(x => x.I(StructuredDouble))
        .Assert(async structuredTask => Assert.Equal(10, await structuredTask));

    [Fact]
    public Task PropagatesAsyncException() =>
        Arrange(() => 1)
        .Act(x => x.I(StructuredThrow))
        .Assert(async structuredTask =>
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await structuredTask);
            Assert.Equal("boom", ex.Message);
        });

    [Fact]
    public Task PropagatesFaultedTask() =>
        Arrange(() => 1)
        .Act(x => x.I(StructuredFaulted))
        .Assert(async structuredTask =>
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await structuredTask);
            Assert.Equal("faulted", ex.Message);
        });

    // ---------- eager, exactly-once evaluation ----------

    [Fact]
    public async Task InvokesFuncExactlyOnceEagerly()
    {
        int calls = 0;
        StructuredTask<int> CountingFunc(int v) { calls++; return v.I<int, int>(w => Task.FromResult(w * 2)); }

        var wrapper = 5.I(CountingFunc);
        Assert.Equal(1, calls);            // func ran eagerly during I(...), before the first await

        Assert.Equal(10, await wrapper);   // result propagates
        Assert.Equal(1, calls);            // awaiting does not re-invoke func
    }

    [Fact]
    public async Task CanBeAwaitedMultipleTimes()
    {
        var wrapper = 5.I(StructuredDouble);
        Assert.Equal(10, await wrapper);
        Assert.Equal(10, await wrapper);   // the wrapper exposes a normal cached task, not a single-shot source
    }

    // ---------- CancellationTokenSource ownership / disposal ----------

    [Fact]
    public void AdoptsInnerCancellationTokenSource()
    {
        StructuredTask<int> inner = null!;
        StructuredTask<int> Capture(int v) { inner = v.I<int, int>(w => Task.FromResult(w)); return inner; }

        var wrapper = 5.I(Capture);

        // Sharing the source means cancelling through the wrapper reaches the work func started.
        Assert.Same(inner.CancellationTokenSource, wrapper.CancellationTokenSource);
    }

    [Fact]
    public void TransfersDisposalOwnershipToWrapper()
    {
        StructuredTask<int> inner = null!;
        StructuredTask<int> Capture(int v) { inner = v.I<int, int>(w => Task.FromResult(w)); return inner; }

        var wrapper = 5.I(Capture);

        // Ownership moved to the wrapper, so disposing the inner handle is a no-op and the shared
        // CancellationTokenSource stays alive.
        inner.Dispose();
        Assert.Null(Record.Exception(() => wrapper.CancellationTokenSource.Token.ThrowIfCancellationRequested()));

        // Disposing the wrapper disposes the shared source exactly once.
        wrapper.Dispose();
        Assert.Throws<ObjectDisposedException>(() => wrapper.CancellationTokenSource.Token.ThrowIfCancellationRequested());
    }

    [Fact]
    public Task CancellingWrapperCtsCancelsInnerWork() =>
        Arrange(() => 1)
        .Act(x =>
        {
            var structuredTask = x.I(DelayedChain);
            structuredTask.CancellationTokenSource.Cancel();
            return structuredTask;
        })
        .Assert(async structuredTask => await Assert.ThrowsAsync<OperationCanceledException>(async () => await structuredTask));

    // ---------- the same contract through the generated tuple overloads ----------

    [Fact]
    public Task Tuple_PropagatesResult() =>
        Arrange(() => (2, 3))
        .Act(x => x.I(StructuredSum))
        .Assert(async structuredTask => Assert.Equal(5, await structuredTask));

    [Fact]
    public Task Tuple_PropagatesAsyncException() =>
        Arrange(() => (2, 3))
        .Act(x => x.I(StructuredSumThrow))
        .Assert(async structuredTask =>
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await structuredTask);
            Assert.Equal("tuple boom", ex.Message);
        });

    [Fact]
    public void Tuple_TransfersDisposalOwnershipToWrapper()
    {
        StructuredTask<int> inner = null!;
        StructuredTask<int> Capture(int a, int b) { inner = (a + b).I<int, int>(w => Task.FromResult(w)); return inner; }

        var wrapper = (2, 3).I(Capture);

        inner.Dispose();
        Assert.Null(Record.Exception(() => wrapper.CancellationTokenSource.Token.ThrowIfCancellationRequested()));

        wrapper.Dispose();
        Assert.Throws<ObjectDisposedException>(() => wrapper.CancellationTokenSource.Token.ThrowIfCancellationRequested());
    }

    [Fact]
    public Task Tuple_HigherArity_PropagatesResult() =>
        Arrange(() => (1, 2, 3, 4))
        .Act(x => x.I(StructuredSum4))
        .Assert(async structuredTask => Assert.Equal(10, await structuredTask));

    // ---------- helpers ----------
    // Each returns StructuredTask<int> as a method group, which is what binds the call site above to the
    // value->StructuredTask wrap overload. Their bodies use explicit type arguments (.I<int, int>) and a
    // Task-returning delegate, which binds to the value->Task overload that builds the inner StructuredTask.

    private static StructuredTask<int> StructuredDouble(int v) => v.I<int, int>(w => Task.FromResult(w * 2));

    private static StructuredTask<int> StructuredThrow(int v) =>
        v.I<int, int>(async _ => { await Task.Yield(); throw new InvalidOperationException("boom"); });

    private static StructuredTask<int> StructuredFaulted(int v) =>
        v.I<int, int>(_ => Task.FromException<int>(new InvalidOperationException("faulted")));

    private static StructuredTask<int> DelayedChain(int v) =>
        v.I<int, int>(val => Task.FromResult(val * 2))
         .I(async x => { await Task.Delay(50); return x + 1; })
         .I(x => x + 1);

    private static StructuredTask<int> StructuredSum(int a, int b) => (a + b).I<int, int>(w => Task.FromResult(w));

    private static StructuredTask<int> StructuredSumThrow(int a, int b) =>
        (a + b).I<int, int>(async _ => { await Task.Yield(); throw new InvalidOperationException("tuple boom"); });

    private static StructuredTask<int> StructuredSum4(int a, int b, int c, int d) =>
        (a + b + c + d).I<int, int>(w => Task.FromResult(w));
}
