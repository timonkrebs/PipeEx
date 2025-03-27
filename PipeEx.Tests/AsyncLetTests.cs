using PipeEx.StructuredConcurrency;
using static BunsenBurner.ArrangeActAssert;

namespace PipeEx.Tests;

public class AsyncLetTests
{
    [Fact]
    public Task Test1() =>
        Arrange(() => 2)
        .Act<int, int>(x =>
            x.Let(() => Task.FromResult(10))
            .Await((x, y) => x * y))
        .Assert(r => Assert.Equal(20, r));

    [Fact]
    public Task Test2() =>
        Arrange(() => 1)
        .Act<int, int>(x =>
            x.Let(() => Task.FromResult(10))
            .Let(() => Task.FromResult(2))
            .Await((x, y, z) => x * y * z))
        .Assert(r => Assert.Equal(20, r));

    [Fact]
    public Task Test3_DiscardLast() =>
        Arrange(() => 1)
        .Act<int, int>(x =>
            x.Let(() => Task.FromResult(10))
            .Let(() => Task.FromResult(2))
            .Await((x, y, _z) => x * y))
        .Assert(r => Assert.Equal(10, r));

    [Fact]
    public Task Test4_DiscardMiddle() =>
        Arrange(() => 1)
        .Act<int, int>(x =>
            x.Let(() => Task.FromResult(10))
            .Let(() => Task.FromResult(2))
            .Await((x, _y, z) => x * z))
        .Assert(r => Assert.Equal(2, r));

    [Fact]
    public Task Test5_DiscardMiddleAndLast() =>
        Arrange(() => 1)
        .Act<int, int>(x =>
            x.Let(() => Task.FromResult(10))
            .Let(() => Task.FromResult(2))
            .Await((x,
            _y,

            _z) => x * _y * _z))
        .Assert(r => Assert.Equal(0, r));

    [Fact]
    public Task Test6_DiscardMiddleAndLast() =>
        Arrange(() => 1)
        .Act<int, int>(x =>
            x.Let(() => Task.FromResult(10))
            .Let(() => Task.FromResult(2))
            .Await((x,
            _y,

            _z) => x + _y + _z))
        .Assert(r => Assert.Equal(1, r));

    [Fact]
    public Task Test7_DiscardMiddleAndLast() =>
        Arrange(() => 1)
        .Act<int, int>(x =>
            x.Let(() => Task.FromResult(10))
            .Let(() => Task.FromResult(2))
            .Await((x,
            _,

            _) => x))
        .Assert(r => Assert.Equal(1, r));

    [Fact]
    public Task Test8_DiscardFirst() =>
        Arrange(() => 1)
        .Act<int, int>(x =>
            x.Let(() => Task.FromResult(10))
            .Let(() => Task.FromResult(2))
            .Await((_x, y, z) => z * y))
        .Assert(r => Assert.Equal(20, r));

    [Fact]
    public Task Let_Source_FuncReturnsStructuredTask_Success() =>
        Arrange(() => 2)
        .Act(x =>
            x.Let(src =>
                new StructuredTask<int>(Task.FromResult(src * 5), CancellationToken.None))
            .Await((original, deferred) => original + deferred))
        .Assert(async resultTask =>
        {
            var result = await resultTask;
            Assert.Equal(12, result);
        });

    [Fact]
    public Task Let_StructuredTaskSource_FuncReturnsStructuredTask_Success() =>
        Arrange(() => new StructuredTask<int>(Task.FromResult(3), CancellationToken.None))
        .Act(st =>
            st.Let(src =>
                new StructuredTask<int>(Task.FromResult(src * 4), CancellationToken.None))
            .Await((original, deferred) => original * deferred))
        .Assert(async resultTask =>
        {
            var result = await resultTask;
            Assert.Equal(36, result);
        });
}