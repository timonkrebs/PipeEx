using PipeEx.ConditionalExpressions;

using static BunsenBurner.ArrangeActAssert;

namespace PipeEx.Tests;

public class IfExpressionsValueTests
{
    [Fact]
    public Task Test_If_Values_PredicateTrue() =>
    Arrange(() => 2)
    .Act(x => x.If(val => val <= 2, "Woohoo", "Noooo"))
    .Assert(result => Assert.Equal("Woohoo", result));

    [Fact]
    public Task Test_If_Values_PredicateFalse() =>
    Arrange(() => 3)
    .Act(x => x.If(val => val <= 2, "Woohoo", "Noooo"))
    .Assert(result => Assert.Equal("Noooo", result));

    [Fact]
    public Task Test_If_Values_TaskSource() =>
    Arrange(() => 2)
    .Act(async x => await Task.FromResult(x).If(val => val <= 2, "Woohoo", "Noooo"))
    .Assert(result => Assert.Equal("Woohoo", result));
}

public class IfExpressionsAsyncTests
{
    private static async Task<int> AddTwoAsync(int x)
    {
        await Task.Yield();
        return x + 2;
    }

    private static async Task<string> ToWordAsync(string word)
    {
        await Task.Yield();
        return word;
    }

    [Fact]
    public Task Test_If_AsyncTransform_PredicateTrue() =>
    Arrange(() => 1)
    .Act(async x => await x.If(val => val <= 2, AddTwoAsync).Else(val => val))
    .Assert(result => Assert.Equal(3, result));

    [Fact]
    public Task Test_If_AsyncTransform_PredicateFalse_ElseReturnsSource() =>
    Arrange(() => 5)
    .Act(async x => await x.If(val => val <= 2, AddTwoAsync).Else(val => val))
    .Assert(result => Assert.Equal(5, result));

    [Fact]
    public Task Test_If_AsyncTransform_PredicateFalse_TransformNotInvoked()
    {
        var invoked = false;
        return Arrange(() => 5)
        .Act(async x => await x.If(val => val <= 2, async val => { invoked = true; await Task.Yield(); return val; }).Else(val => val))
        .Assert(_ => Assert.False(invoked));
    }

    [Fact]
    public Task Test_IfElse_BothAsync_PredicateTrue() =>
    Arrange(() => 2)
    .Act(async x => await x.If(val => val <= 2, _ => ToWordAsync("Woohoo"), _ => ToWordAsync("Noooo")))
    .Assert(result => Assert.Equal("Woohoo", result));

    [Fact]
    public Task Test_IfElse_BothAsync_PredicateFalse() =>
    Arrange(() => 3)
    .Act(async x => await x.If(val => val <= 2, _ => ToWordAsync("Woohoo"), _ => ToWordAsync("Noooo")))
    .Assert(result => Assert.Equal("Noooo", result));

    [Fact]
    public Task Test_IfElse_AsyncThen_SyncElse() =>
    Arrange(() => 1)
    .Act(async x => await x.If(val => val <= 2, AddTwoAsync, val => val - 2))
    .Assert(result => Assert.Equal(3, result));

    [Fact]
    public Task Test_IfElse_SyncThen_AsyncElse() =>
    Arrange(() => 5)
    .Act(async x => await x.If(val => val <= 2, val => val - 2, AddTwoAsync))
    .Assert(result => Assert.Equal(7, result));

    [Fact]
    public Task Test_If_TaskSource_SyncTransform() =>
    Arrange(() => 1)
    .Act(async x => await Task.FromResult(x).If(val => val <= 2, val => val + 2).Else(val => val))
    .Assert(result => Assert.Equal(3, result));

    [Fact]
    public Task Test_If_TaskSource_AsyncTransform() =>
    Arrange(() => 1)
    .Act(async x => await Task.FromResult(x).If(val => val <= 2, AddTwoAsync).Else(val => val))
    .Assert(result => Assert.Equal(3, result));

    [Fact]
    public Task Test_IfElse_TaskSource_SyncTransforms() =>
    Arrange(() => 7)
    .Act(async x => await Task.FromResult(x).If(val => val % 2 == 0, val => "even", val => "odd"))
    .Assert(result => Assert.Equal("odd", result));

    [Fact]
    public Task Test_IfElse_TaskSource_BothAsync() =>
    Arrange(() => 2)
    .Act(async x => await Task.FromResult(x).If(val => val <= 2, _ => ToWordAsync("Woohoo"), _ => ToWordAsync("Noooo")))
    .Assert(result => Assert.Equal("Woohoo", result));

    [Fact]
    public Task Test_IfElse_TaskSource_AsyncThen_SyncElse() =>
    Arrange(() => 1)
    .Act(async x => await Task.FromResult(x).If(val => val <= 2, AddTwoAsync, val => val - 2))
    .Assert(result => Assert.Equal(3, result));

    [Fact]
    public Task Test_IfElse_TaskSource_SyncThen_AsyncElse() =>
    Arrange(() => 5)
    .Act(async x => await Task.FromResult(x).If(val => val <= 2, val => val - 2, AddTwoAsync))
    .Assert(result => Assert.Equal(7, result));
}

public class WhenExpressionsAsyncTests
{
    [Fact]
    public Task Test_When_AsyncAction_PredicateTrue_ActionExecutes()
    {
        var executed = false;
        return Arrange(() => 10)
        .Act(async x => await x.When(val => val > 5, async _ => { await Task.Yield(); executed = true; }))
        .Assert(result =>
        {
            Assert.True(executed);
            Assert.Equal(10, result);
        });
    }

    [Fact]
    public Task Test_When_AsyncAction_PredicateFalse_ActionNotExecuted()
    {
        var executed = false;
        return Arrange(() => 3)
        .Act(async x => await x.When(val => val > 5, async _ => { await Task.Yield(); executed = true; }))
        .Assert(result =>
        {
            Assert.False(executed);
            Assert.Equal(3, result);
        });
    }

    [Fact]
    public Task Test_When_TaskSource_SyncAction()
    {
        var executed = false;
        return Arrange(() => 10)
        .Act(async x => await Task.FromResult(x).When(val => val > 5, _ => executed = true))
        .Assert(result =>
        {
            Assert.True(executed);
            Assert.Equal(10, result);
        });
    }

    [Fact]
    public Task Test_When_TaskSource_AsyncAction()
    {
        var executed = false;
        return Arrange(() => 10)
        .Act(async x => await Task.FromResult(x).When(val => val > 5, async _ => { await Task.Yield(); executed = true; }))
        .Assert(result =>
        {
            Assert.True(executed);
            Assert.Equal(10, result);
        });
    }
}

public class GuardExpressionsAsyncTests
{
    [Fact]
    public Task Test_Guard_AsyncAction_PredicateTrue_ActionExecutes()
    {
        var executed = false;
        return Arrange(() => 10)
        .Act(async x => await x.Guard(val => val > 5, async _ => { await Task.Yield(); executed = true; }))
        .Assert(result =>
        {
            Assert.True(executed);
            Assert.False(result.Skip);
        });
    }

    [Fact]
    public Task Test_Guard_AsyncAction_PredicateFalse_Skips()
    {
        var executed = false;
        return Arrange(() => 3)
        .Act(async x => await x.Guard(val => val > 5, async _ => { await Task.Yield(); executed = true; }))
        .Assert(result =>
        {
            Assert.False(executed);
            Assert.True(result.Skip);
        });
    }

    [Fact]
    public Task Test_Guard_TaskSource_SyncAction()
    {
        var executed = false;
        return Arrange(() => 10)
        .Act(async x => await Task.FromResult(x).Guard(val => val > 5, _ => executed = true))
        .Assert(result =>
        {
            Assert.True(executed);
            Assert.False(result.Skip);
        });
    }

    [Fact]
    public Task Test_Guard_Chained_MixedSyncAsync()
    {
        var executed1 = false;
        var executed2 = false;
        return Arrange(() => 10)
        .Act(async x => await x.Guard(val => val > 5, async _ => { await Task.Yield(); executed1 = true; })
                               .Guard(val => val < 20, _ => executed2 = true))
        .Assert(_ =>
        {
            Assert.True(executed1);
            Assert.True(executed2);
        });
    }

    [Fact]
    public Task Test_Guard_Chained_SkipPropagates()
    {
        var executed2 = false;
        return Arrange(() => 3)
        .Act(async x => await x.Guard(val => val > 5, async _ => { await Task.Yield(); })
                               .Guard(val => val < 20, async _ => { await Task.Yield(); executed2 = true; }))
        .Assert(result =>
        {
            Assert.False(executed2);
            Assert.True(result.Skip);
        });
    }

    [Fact]
    public Task Test_Guard_Else_AsyncAction_ExecutesWhenSkipped()
    {
        var elseExecuted = false;
        return Arrange(() => 3)
        .Act(async x => await x.Guard(val => val > 5, _ => { })
                               .Else(async _ => { await Task.Yield(); elseExecuted = true; }))
        .Assert(result =>
        {
            Assert.True(elseExecuted);
            Assert.Equal(3, result);
        });
    }

    [Fact]
    public Task Test_Guard_TaskChain_Else_SyncAction()
    {
        var elseExecuted = false;
        return Arrange(() => 3)
        .Act(async x => await Task.FromResult(x).Guard(val => val > 5, async _ => { await Task.Yield(); })
                                                .Else(_ => elseExecuted = true))
        .Assert(result =>
        {
            Assert.True(elseExecuted);
            Assert.Equal(3, result);
        });
    }

    [Fact]
    public Task Test_Guard_TaskChain_Else_AsyncAction_NotExecutedWhenNotSkipped()
    {
        var elseExecuted = false;
        return Arrange(() => 10)
        .Act(async x => await Task.FromResult(x).Guard(val => val > 5, _ => { })
                                                .Else(async _ => { await Task.Yield(); elseExecuted = true; }))
        .Assert(result =>
        {
            Assert.False(elseExecuted);
            Assert.Equal(10, result);
        });
    }
}

public class IfChainTests
{
    [Fact]
    public Task Test_IfChain_FirstBranchMatches() =>
    Arrange(() => 95)
    .Act(x => x.If(s => s >= 90, "A")
               .ElseIf(s => s >= 80, "B")
               .Else("F"))
    .Assert(result => Assert.Equal("A", result));

    [Fact]
    public Task Test_IfChain_LaterBranchMatches() =>
    Arrange(() => 85)
    .Act(x => x.If(s => s >= 90, "A")
               .ElseIf(s => s >= 80, "B")
               .Else("F"))
    .Assert(result => Assert.Equal("B", result));

    [Fact]
    public Task Test_IfChain_NoBranchMatches_ElseWins() =>
    Arrange(() => 42)
    .Act(x => x.If(s => s >= 90, "A")
               .ElseIf(s => s >= 80, "B")
               .Else("F"))
    .Assert(result => Assert.Equal("F", result));

    [Fact]
    public Task Test_IfChain_Transforms() =>
    Arrange(() => 4)
    .Act(x => x.If(s => s % 2 == 0, s => s * 10)
               .ElseIf(s => s % 3 == 0, s => s * 100)
               .Else(s => s))
    .Assert(result => Assert.Equal(40, result));

    [Fact]
    public Task Test_IfChain_ElseTransform_ReceivesSource() =>
    Arrange(() => 7)
    .Act(x => x.If(s => s % 2 == 0, s => s * 10)
               .ElseIf(s => s % 3 == 0, s => s * 100)
               .Else(s => s + 1))
    .Assert(result => Assert.Equal(8, result));

    [Fact]
    public Task Test_IfChain_FirstMatchWins_LaterPredicatesNotEvaluated()
    {
        var laterPredicateEvaluated = false;
        var laterTransformEvaluated = false;
        return Arrange(() => 95)
        .Act(x => x.If(s => s >= 90, "A")
                   .ElseIf(s => { laterPredicateEvaluated = true; return s >= 80; }, s => { laterTransformEvaluated = true; return "B"; })
                   .Else("F"))
        .Assert(result =>
        {
            Assert.Equal("A", result);
            Assert.False(laterPredicateEvaluated);
            Assert.False(laterTransformEvaluated);
        });
    }

    [Fact]
    public Task Test_IfChain_NonMatchingTransformNotEvaluated()
    {
        var firstTransformEvaluated = false;
        return Arrange(() => 85)
        .Act(x => x.If(s => s >= 90, s => { firstTransformEvaluated = true; return "A"; })
                   .ElseIf(s => s >= 80, "B")
                   .Else("F"))
        .Assert(result =>
        {
            Assert.Equal("B", result);
            Assert.False(firstTransformEvaluated);
        });
    }
}

public class IfChainAsyncTests
{
    private static async Task<string> WordAsync(string word)
    {
        await Task.Yield();
        return word;
    }

    [Fact]
    public Task Test_IfChain_AsyncTransform_Matches() =>
    Arrange(() => 95)
    .Act(async x => await x.If(s => s >= 90, _ => WordAsync("A"))
                           .ElseIf(s => s >= 80, _ => WordAsync("B"))
                           .Else(_ => WordAsync("F")))
    .Assert(result => Assert.Equal("A", result));

    [Fact]
    public Task Test_IfChain_AsyncElseIfBranch_Matches() =>
    Arrange(() => 85)
    .Act(async x => await x.If(s => s >= 90, _ => WordAsync("A"))
                           .ElseIf(s => s >= 80, _ => WordAsync("B"))
                           .Else(_ => WordAsync("F")))
    .Assert(result => Assert.Equal("B", result));

    [Fact]
    public Task Test_IfChain_AsyncElse_Wins() =>
    Arrange(() => 42)
    .Act(async x => await x.If(s => s >= 90, _ => WordAsync("A"))
                           .ElseIf(s => s >= 80, _ => WordAsync("B"))
                           .Else(_ => WordAsync("F")))
    .Assert(result => Assert.Equal("F", result));

    [Fact]
    public Task Test_IfChain_TaskSource_SyncBranches() =>
    Arrange(() => 85)
    .Act(async x => await Task.FromResult(x).If(s => s >= 90, "A")
                                            .ElseIf(s => s >= 80, "B")
                                            .Else("F"))
    .Assert(result => Assert.Equal("B", result));

    [Fact]
    public Task Test_IfChain_TaskSource_MixedBranches() =>
    Arrange(() => 42)
    .Act(async x => await Task.FromResult(x).If(s => s >= 90, _ => WordAsync("A"))
                                            .ElseIf(s => s >= 80, s => $"B{s}")
                                            .Else("F"))
    .Assert(result => Assert.Equal("F", result));

    [Fact]
    public Task Test_IfChain_AsyncBranch_OnlyMatchedBranchAwaited()
    {
        var unmatchedInvoked = false;
        return Arrange(() => 85)
        .Act(async x => await x.If(s => s >= 90, async _ => { unmatchedInvoked = true; await Task.Yield(); return "A"; })
                               .ElseIf(s => s >= 80, _ => WordAsync("B"))
                               .Else(_ => WordAsync("F")))
        .Assert(result =>
        {
            Assert.Equal("B", result);
            Assert.False(unmatchedInvoked);
        });
    }

    [Fact]
    public Task Test_IfChain_TaskChain_ElseValue_NotUsedWhenMatched() =>
    Arrange(() => 95)
    .Act(async x => await Task.FromResult(x).If(s => s >= 90, _ => WordAsync("A"))
                                            .ElseIf(s => s >= 80, "B")
                                            .Else("F"))
    .Assert(result => Assert.Equal("A", result));
}
