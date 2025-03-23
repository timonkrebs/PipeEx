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
}