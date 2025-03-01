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

    private Task<int> Chain(int i) => i.I(y => Task.FromResult(y))
                   .I(y => y + 2)
                   .I(Task.FromResult<int>);

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
    public Task Test8_TupleReturnMixedAsync1() => Arrange(() => 1)
        .Act(x => (Task<(int,int)>)x.I(a => Task.FromResult(a + 1), a => a * 2))
        .Assert(r => Assert.Equal((2, 2), r));


    [Fact]
    public Task Test9_TupleReturnMixedAsync2() => Arrange(() => 1)
        .Act(x => (Task<(int,int)>)x.I(a => Task.FromResult(a + 1), a => Task.FromResult(a * 2)))
        .Assert(r => Assert.Equal((2, 2), r));

    [Fact]
    public Task Test10_TupleReturnTupleDestructuringMixedAsync1() => Arrange(() => (1, 2))
        .Act(x => (Task<(int,int)>)x.I((a, b) => Task.FromResult(a + 1), (a, b) => b * 2))
        .Assert(r => Assert.Equal((2, 4), r));

    [Fact]
    public Task Test11_TupleReturnTupleDestructuringMixedAsync2() => Arrange(() => (1, 2))
       .Act(x => (Task<(int,int)>)x.I((a, b) => Task.FromResult(a + 1), (a, b) => Task.FromResult(b * 2)))
       .Assert(r => Assert.Equal((2, 4), r));

    [Fact]
    public Task Test12_BranchingAsync() => Arrange(() => 2)
       .Act(Calc)
       .Assert(r => Assert.Equal(5, r));

    private Task<int> Calc(int x) => x.I(FuncXAsync,FuncYAsync)
                             .I((x, y) => x + y)
                             .I(FuncY);

    private Func<int, Task<int>> FuncXAsync = async x => await Task.FromResult(x);
    private Func<int, Task<int>> FuncYAsync = x => Task.FromResult(x);
    private int FuncY(int x) => x + 1;
}