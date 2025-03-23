using PipeEx.StructuredConcurrency;
using static BunsenBurner.ArrangeActAssert;

namespace PipeEx.Tests;

public class AsyncLetTests
{
    [Fact]
    public Task Test1_InnerTaskExceptionAsync() =>
        Arrange(() => 2)
        .Act<int, int>(x => 
            x.Let(() => Task.FromResult(10))
            .Await((x, y) =>  x * y))
        .Assert(r => Assert.Equal(20, r));
}