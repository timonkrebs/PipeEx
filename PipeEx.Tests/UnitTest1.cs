using System.Runtime.CompilerServices;
using PipeEx;
using static BunsenBurner.ArrangeActAssert;

namespace PipeEx.Tests;

public class UnitTest1
{
    [Fact]
    public Task Test1() => Arrange(() => 1)
        .Act(x => x.I(y => y + 2))
        .Assert(r => Assert.Equal(3, r));

    [Fact]
    public Task Test2() => Arrange(() => 1)
        .Act(x => x.I(y => Task.FromResult(y)))
        .Assert(r => Assert.Equal(1, 1));

    [Fact]
    public Task Test3() => Arrange(() => 1)
        .Act(x => x.I(y => Task.FromResult(y))
                   .I(y => y + 2))
        .Assert(r => Assert.Equal(3, r));
}