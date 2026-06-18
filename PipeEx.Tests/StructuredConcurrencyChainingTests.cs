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
        .Assert(r => Assert.Equal(6, r));

    [Fact]
    public Task StructuredTaskSource_AsyncFunc_Success() =>
        Arrange(() => new StructuredTask<int>(Task.FromResult(5), CancellationToken.None))
        .Act(s => s.I(v => Task.FromResult(v + 1)))
        .Assert(r => Assert.Equal(6, r));

    [Fact]
    public Task TaskSource_StructuredTaskFunc_Success() =>
        Arrange(() => Task.FromResult(5))
        .Act(s => s.I(StructuredDouble))
        .Assert(r => Assert.Equal(10, r));

    [Fact]
    public Task StructuredTaskSource_StructuredTaskFunc_Success() =>
        Arrange(() => new StructuredTask<int>(Task.FromResult(5), CancellationToken.None))
        .Act(s => s.I(StructuredDouble))
        .Assert(r => Assert.Equal(10, r));

    [Fact]
    public Task TupleTaskSource_StructuredTaskFunc_Success() =>
        Arrange(() => Task.FromResult((2, 3)))
        .Act(s => s.I<int, int, int>((a, b) => StructuredDouble(a + b)))
        .Assert(r => Assert.Equal(10, r));

    [Fact]
    public Task TupleStructuredTaskSource_StructuredTaskFunc_Success() =>
        Arrange(() => new StructuredTask<(int, int)>(Task.FromResult((2, 3)), CancellationToken.None))
        .Act(s => s.I<int, int, int>((a, b) => StructuredDouble(a + b)))
        .Assert(r => Assert.Equal(10, r));

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

    private static StructuredTask<int> StructuredDouble(int v) => v.I<int, int>(w => Task.FromResult(w * 2));
    private static StructuredTask<int> StructuredThrow(int v) => v.I<int, int>(async _ => { await Task.Yield(); throw new InvalidOperationException("boom"); });
    private static async Task<int> ThrowAsync() { await Task.Yield(); throw new InvalidOperationException("deferred boom"); }
}
