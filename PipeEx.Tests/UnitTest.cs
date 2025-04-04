using PipeEx.StructuredConcurrency;
using static BunsenBurner.ArrangeActAssert;

namespace PipeEx.Tests;

public class UnitTest
{
    [Fact]
    public Task Test1() => Arrange(() => 1)
        .Act(x => x.I(y => y + 2))
        .Assert(r => Assert.Equal(3, r));

    [Fact]
    public Task Test2() => Arrange(() => 1)
        .Act(x => x.I(y => Task.FromResult(y)))
        .Assert(r => Assert.Equal(1, r));

    [Fact]
    public Task Test3() => Arrange(() => 1)
        .Act(Chain)
        .Assert(r => Assert.Equal(3, r));

    [Fact]
    public Task Test4_TupleDestructuring() => Arrange(() => (1, 2))
        .Act(x => x.I((a, b) => a + b))
        .Assert(r => Assert.Equal(3, r));

    [Fact]
    public Task Test5_TupleDestructuringAsync() => Arrange(() => (1, 2))
        .Act(x => x.I(async (a, b) => await Task.FromResult(a + b)))
        .Assert(r => Assert.Equal(3, r));

    [Fact]
    public Task Test6_TupleDestructuringTaskSource() => Arrange(() => Task.FromResult((1, 2)))
        .Act(x => x.I((a, b) => a + b))
        .Assert(r => Assert.Equal(3, r));

    [Fact]
    public Task Test7_TupleDestructuringTaskSourceAsync() => Arrange(() => Task.FromResult((1, 2)))
        .Act(x => x.I(async (a, b) => await Task.FromResult(a + b)))
        .Assert(r => Assert.Equal(3, r));

    private Task<int> Chain(int i) => i.I(y => Task.FromResult(y))
                   .I(y => y + 2)
                   .I(Task.FromResult<int>);
}