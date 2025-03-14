using PipeEx;
using PipeEx.ConditionalExpressions;

using static BunsenBurner.ArrangeActAssert;

public class CoreTests
{
    [Fact]
    public Task Test_Core_I_SimpleTransformation() =>
    Arrange(() => 5)
    .Act(x => x.I(val => val * 2))
    .Assert(result => Assert.Equal(10, result));

    [Fact]
    public Task Test_Core_I_MultipleTransformations() =>
    Arrange(() => "hello")
    .Act(x => x.I(str => str.ToUpper()).I(str => str + " world"))
    .Assert(result => Assert.Equal("HELLO world", result));

    [Fact]
    public Task Test_Core_I_TransformationReturnsNull() =>
    Arrange(() => "test")
    .Act(x => x.I(str => (string?)null))
    .Assert(Assert.Null);
}

public class TupleDestructuringTests
{
    [Fact]
    public Task Test_TupleDestructuring_I_TwoElements() =>
    Arrange(() => (2, 3))
    .Act(x => x.I((a, b) => a * b))
    .Assert(result => Assert.Equal(6, result));

    [Fact]
    public Task Test_TupleDestructuring_I_ThreeElements() =>
    Arrange(() => (1, 2, 3))
    .Act(x => x.I((a, b, c) => a + b + c))
    .Assert(result => Assert.Equal(6, result));
}

public class IfExpressionsTests
{
    [Fact]
    public Task Test_IfExpressions_If_PredicateTrue() =>
    Arrange(() => 10)
    .Act(x => x.If(val => val > 5, val => val * 2))
    .Assert(result => Assert.Equal(20, result));

    [Fact]
    public Task Test_IfExpressions_If_PredicateFalse() =>
    Arrange(() => 3)
    .Act(x => x.If(val => val > 5, val => val * 2))
    .Assert(result => Assert.Equal(3, result));

    [Fact]
    public Task Test_IfExpressions_IfElse_PredicateTrue() =>
    Arrange(() => 8)
    .Act(x => x.If(val => val % 2 == 0, val => "even", val => "odd"))
    .Assert(result => Assert.Equal("even", result));

    [Fact]
    public Task Test_IfExpressions_IfElse_PredicateFalse() =>
    Arrange(() => 7)
    .Act(x => x.If(val => val % 2 == 0, val => "even", val => "odd"))
    .Assert(result => Assert.Equal("odd", result));
}

public class GuardExpressionsTests
{
    [Fact]
    public Task Test_GuardExpressions_Guard_PredicateTrue_ActionExecutes()
    {
        var executed = false;
        return Arrange(() => 10)
        .Act(x => x.Guard(val => val > 5, _ => executed = true))
        .Assert(_ => Assert.True(executed));
    }

    [Fact]
    public Task Test_GuardExpressions_Guard_PredicateTrue_ReturnsCorrectResult() =>
    Arrange(() => 10)
        .Act(x => x.Guard(val => val > 5, _ => { }))
        .Assert(result =>
        {
            Assert.NotNull(result);
            Assert.Equal(10, result.Value);
            Assert.False(result.Skip);
        });

    [Fact]
    public Task Test_GuardExpressions_Guard_PredicateFalse_ActionDoesNotExecute()
    {
        var executed = false;
        return Arrange(() => 3)
        .Act(x => x.Guard(val => val > 5, _ => executed = true))
        .Assert(_ => Assert.False(executed));
    }

    [Fact]
    public Task Test_GuardExpressions_Guard_PredicateFalse_ReturnsSkipResult() => Arrange(() => 3)
    .Act(x => x.Guard(val => val > 5, _ => { }))
    .Assert(result =>
    {
        Assert.NotNull(result);
        Assert.Equal(3, result.Value);
        Assert.True(result.Skip);
    });

    [Fact]
    public Task Test_GuardExpressions_Guard_Chained_FirstPredicateTrue_SecondPredicateTrue()
    {
        var executed1 = false;
        var executed2 = false;
        return Arrange(() => 10)
       .Act(x => x.Guard(val => val > 5, _ => executed1 = true)
                  .Guard(val => val < 20, _ => executed2 = true))
       .Assert(_ =>
       {
           Assert.True(executed1);
           Assert.True(executed2);
       });
    }

    [Fact]
    public Task Test_GuardExpressions_Guard_Chained_FirstPredicateFalse_SecondPredicateNotEvaluated()
    {
        var executed1 = false;
        var executed2 = false;
        return Arrange(() => 3)
       .Act(x => x.Guard(val => val > 5, _ => executed1 = true)
                  .Guard(val => val < 20, _ => executed2 = true)) // This won't be executed
       .Assert(_ =>
       {
           Assert.False(executed1);
           Assert.False(executed2);  // Second action should not have executed
       });
    }

    [Fact]
    public Task Test_GuardExpressions_Guard_Chained_FirstPredicateFalse_ReturnsCorrectResult() =>
    Arrange(() => 3)
    .Act(x => x.Guard(val => val > 5, _ => { })
                .Guard(val => val < 20, _ => { }))
    .Assert(result =>
    {
        Assert.NotNull(result);     // Should not be null
        Assert.Equal(3, result.Value); // Should have the initial Value
        Assert.True(result.Skip);      // Check the Skip)
    });

    [Fact]
    public Task Test_GuardExpressions_Else_PreviousGuardSkipped()
    {
        var elseExecuted = false;
        return Arrange(() => 3)
       .Act(x => x.Guard(val => val > 5, _ => { })
                  .Else(_ => elseExecuted = true))
       .Assert(_ => Assert.True(elseExecuted));
    }

    [Fact]
    public Task Test_GuardExpressions_Else_PreviousGuardNotSkipped()
    {
        var elseExecuted = false;
        return Arrange(() => 10)
        .Act(x => x.Guard(val => val > 5, _ => { })
                .Else(_ => elseExecuted = true))
        .Assert(_ => Assert.False(elseExecuted));
    }
}

public class WhenExpressionsTests
{
    [Fact]
    public Task Test_WhenExpressions_When_PredicateTrue_ActionExecutes()
    {
        var executed = false;
        return Arrange(() => 10)
        .Act(x => x.When(val => val > 5, _ => executed = true))
        .Assert(_ => Assert.True(executed));
    }

    [Fact]
    public Task Test_WhenExpressions_When_PredicateFalse_ActionDoesNotExecute()
    {
        var executed = false;
        return Arrange(() => 3)
       .Act(x => x.When(val => val > 5, _ => executed = true))
       .Assert(_ => Assert.False(executed));
    }

    [Fact]
    public Task Test_WhenExpressions_When_ReturnsOriginalSource() => Arrange(() => 10)
        .Act(x => x.When(val => val > 5, _ => { }))
        .Assert(result => Assert.Equal(10, result));

    [Fact]
    public Task Test_WhenExpressions_When_Chained()
    {
        var executed1 = false;
        var executed2 = false;
        return Arrange(() => 10)
       .Act(x => x.When(val => val > 5, _ => executed1 = true)
                  .When(val => val < 20, _ => executed2 = true))
       .Assert(_ =>
       {
           Assert.True(executed1);
           Assert.True(executed2);
       });
    }

    [Fact]
    public Task Test_WhenExpressions_When_Chained_FirstPredicateFalse()
    {
        var executed1 = false;
        var executed2 = false;
        return Arrange(() => 3)
       .Act(x => x.When(val => val > 5, _ => executed1 = true)
                  .When(val => val < 20, _ => executed2 = true)) // This *will* be evaluated
       .Assert(_ =>
       {
           Assert.False(executed1);
           Assert.True(executed2); // Second action *should* have executed, different from Guard
       });
    }
}

public class ComplexCoreTests
{
    [Fact]
    public Task Test_Core_I_ChainedWithConditional() =>
    Arrange(() => 10)
    .Act(x => x.I(val => val * 2)
                .If(val => val > 15, val => val + 5, val => val - 5)
                .I(val => val.ToString()))
    .Assert(result => Assert.Equal("25", result));

    [Fact]
    public Task Test_Core_I_ChainedWithGuardAndWhen()
    {
        var guardExecuted = false;
        var whenExecuted = false;

        return Arrange(() => "hello")
            .Act(x => x.I(str => str.ToUpper())
                       .Guard(str => str.StartsWith("H"), _ => guardExecuted = true)
                       .Else(_ => { })
                       .When(str => str.Length >= 5, _ => whenExecuted = true)
                       .I(str => str + " world"))
            .Assert(result =>
            {
                Assert.True(guardExecuted);
                Assert.True(whenExecuted);
                Assert.Equal("HELLO world", result);
            });
    }
}

public class ComplexTupleDestructuringTests
{
    [Fact]
    public Task Test_TupleDestructuring_I_MixedWithConditional() =>
    Arrange(() => (5, 10))
    .Act(x => x.I((a, b) => (a + b).If(sum => sum > 10, sum => sum * 2, sum => sum)))
    .Assert(result => Assert.Equal(30, result));

}

public class ComplexIfExpressionsTests
{
    [Fact]
    public Task Test_IfExpressions_NestedIf() =>
    Arrange(() => 12)
    .Act(x => x.If(val => val > 10,
                    val => val.If(v => v % 2 == 0, v => v / 2, v => v),
                    val => val))
    .Assert(result => Assert.Equal(6, result));

    [Fact]
    public Task Test_IfExpressions_IfElse_ChainedWithGuard()
    {
        var guardExecuted = false;
        return Arrange(() => 7)
          .Act(x => x.If(val => val % 2 == 0, val => "even", val => "odd")
                     .Guard(str => str == "odd", _ => guardExecuted = true))
          .Assert(result =>
          {
              Assert.True(guardExecuted);
              Assert.Equal("odd", result.Value);
              Assert.False(result.Skip);
          });
    }
}

public class ComplexGuardExpressionsTests
{
    [Fact]
    public Task Test_GuardExpressions_Guard_ChainedWithIf()
    {
        var guardExecuted = false;
        return Arrange(() => 15)
        .Act(x => x.Guard(val => val > 10, _ => guardExecuted = true)
                   .If(result => result.Skip, result => 0, result => result.Value * 2))
        .Assert(result =>
        {
            Assert.True(guardExecuted);
            Assert.Equal(30, result);
        });
    }

    [Fact]
    public Task Test_GuardExpressions_Guard_ChainedWithIf_FirstGuardSkipped()
    {
        var guardExecuted = false;
        return Arrange(() => 5)
        .Act(x => x.Guard(val => val > 10, _ => guardExecuted = true)
                    .If(result => result.Skip, result => 0, result => result.Value * 2))
        .Assert(result =>
        {
            Assert.False(guardExecuted);
            Assert.Equal(0, result);
        });
    }

    [Fact]
    public Task Test_GuardExpressions_MultipleElses()
    {
        var else1Executed = false;
        var else2Executed = false;

        return Arrange(() => 5)
        .Act(x => x.Guard(val => val > 10, _ => { })
                   .Else(_ => else1Executed = true)
                   .Guard(val => val < 20, _ => { })
                   .Else(_ => else2Executed = true))
        .Assert(_ =>
        {
            Assert.True(else1Executed);
            Assert.False(else2Executed);
        });
    }

    [Fact]
    public Task Test_GuardExpressions_MultipleGuardsWithTransformations() =>
    Arrange(() => 10)
    .Act(x => x.Guard(val => val > 5, _ => { }).I(x => x.Value * 2)
                .Guard(val => val < 30, _ => { }).I(x => x.Value + 5))
    .Assert(result => Assert.Equal(25, result));

    [Fact]
    public Task Test_GuardExpressions_MultipleGuardsWithTransformations_FirstGuardSkipped() =>
    Arrange(() => 2)
    .Act(x => x.Guard(val => val > 5, _ => { }).I(x => x.Value * 2)
              .Guard(val => val < 30, _ => { }).I(x => x.Value + 5))
    .Assert(result => Assert.Equal(9, result));

    [Fact]
    public Task Test_GuardExpressions_MultipleElses_AllGuardsSkipped()
    {
        var else1Executed = false;
        var else2Executed = false;

        return Arrange(() => 5)
       .Act(x => x.Guard(v => v > 10, _ => { }).Else(_ => else1Executed = true)
                  .Guard(v => v > 2, _ => { }).Else(_ => else2Executed = true))
       .Assert(_ =>
       {
           Assert.True(else1Executed);
           Assert.False(else2Executed);
       });
    }
}

public class ComplexWhenExpressionsTests
{
    [Fact]
    public Task Test_WhenExpressions_ChainedWithIfAndGuard()
    {
        var whenExecuted = false;
        var guardExecuted = false;

        return Arrange(() => "test")
            .Act(x => x.When(str => str.Length > 3, _ => whenExecuted = true)
                       .If(str => str == "test", str => str.ToUpper(), str => str)
                       .Guard(str => str == "TEST", _ => guardExecuted = true))
            .Assert(result =>
            {
                Assert.True(whenExecuted);
                Assert.True(guardExecuted);
                Assert.Equal("TEST", result.Value);
                Assert.False(result.Skip);
            });
    }
}