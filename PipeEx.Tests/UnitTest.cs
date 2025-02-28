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
        .Act(x => x.I(y => Task.FromResult(y))
                   .I(y => y + 2)
                   .I(y => Task.FromResult<int>(y)))
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

    [Fact]
    public Task Test10_TupleReturnMixedAsync1() => Arrange(() => 1)
        .Act(x => x.I(a => Task.FromResult(a + 1), a => a * 2))
        .Assert(r => Assert.Equal((2, 2), r));


    [Fact]
    public Task Test11_TupleReturnMixedAsync3() => Arrange(() => 1)
        .Act(x => x.I(a => Task.FromResult(a + 1), a => Task.FromResult(a * 2)))
        .Assert(r => Assert.Equal((2, 2), r));

    [Fact]
    public Task Test12_TupleReturnTupleDestructuringMixedAsync1() => Arrange(() => (1, 2))
        .Act(x => x.I((a, b) => Task.FromResult(a + 1), (a, b) => b * 2))
        .Assert(r => Assert.Equal((2, 4), r));

    [Fact]
    public Task Test13_TupleReturnTupleDestructuringMixedAsync3() => Arrange(() => (1, 2))
       .Act(x => x.I((a, b) => Task.FromResult(a + 1), (a, b) => Task.FromResult(b * 2)))
       .Assert(r => Assert.Equal((2, 4), r));
}