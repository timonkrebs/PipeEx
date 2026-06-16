using PipeEx.ResultChaining;
using static BunsenBurner.ArrangeActAssert;

namespace PipeEx.Tests;

public class ResultChainingTests
{
    private static Task<Result<int, string>> StartWith(int value) => Task.FromResult(Result<int, string>.Success(value));

    private static Task<Result<int, string>> StartWithFailure(string failure) => Task.FromResult(Result<int, string>.Failure(failure));

    [Fact]
    public Task Result_ImplicitConversions() => Arrange(() => 5)
        .Act(x =>
        {
            Result<int, string> success = x;
            Result<int, string> failure = "boom";
            return (success, failure);
        })
        .Assert(r =>
        {
            Assert.True(r.success.IsSuccess);
            Assert.Equal(5, r.success.SuccessValue);
            Assert.True(r.failure.IsFailure);
            Assert.Equal("boom", r.failure.FailureValue);
        });

    [Fact]
    public Task Result_Match() => Arrange(() => (Result<int, string>.Success(5), Result<int, string>.Failure("boom")))
        .Act(x => (x.Item1.Match(s => $"S{s}", f => $"F{f}"), x.Item2.Match(s => $"S{s}", f => $"F{f}")))
        .Assert(r =>
        {
            Assert.Equal("S5", r.Item1);
            Assert.Equal("Fboom", r.Item2);
        });

    [Fact]
    public Task Result_Switch() => Arrange(() => new List<string>())
        .Act(log =>
        {
            Result<int, string>.Success(5).Switch(s => log.Add($"S{s}"), f => log.Add($"F{f}"));
            Result<int, string>.Failure("boom").Switch(s => log.Add($"S{s}"), f => log.Add($"F{f}"));
            return log;
        })
        .Assert(log => Assert.Equal(new[] { "S5", "Fboom" }, log));

    [Fact]
    public Task Result_AccessingWrongValue_Throws() => Arrange(() => (Result<int, string>.Success(5), Result<int, string>.Failure("boom")))
        .Act(x => x)
        .Assert(r =>
        {
            Assert.Throws<InvalidOperationException>(() => r.Item1.FailureValue);
            Assert.Throws<InvalidOperationException>(() => r.Item2.SuccessValue);
        });

    [Fact]
    public Task ToSuccess_And_ToFailure_StartChains() => Arrange(() => 1)
        .Act(x => (x.ToSuccess<int, string>(), "boom".ToFailure<int, string>()))
        .Assert(r =>
        {
            Assert.Equal(1, r.Item1.SuccessValue);
            Assert.Equal("boom", r.Item2.FailureValue);
        });

    [Fact]
    public Task Then_ChainsAsyncJobs() => Arrange(() => 1)
        .Act(x => StartWith(x)
            .Then(v => Task.FromResult(Result<int, string>.Success(v + 1)))
            .Then(v => Task.FromResult(Result<int, string>.Success(v * 10))))
        .Assert(r => Assert.Equal(20, r.SuccessValue));

    [Fact]
    public Task Then_ChainsMixedSyncAndAsyncJobs() => Arrange(() => 1)
        .Act(x => x.ToSuccess<int, string>()
            .Then(v => Result<int, string>.Success(v + 1))
            .Then(v => Task.FromResult(Result<int, string>.Success(v * 10)))
            .Then(v => Result<int, string>.Success(v + 2)))
        .Assert(r => Assert.Equal(22, r.SuccessValue));

    [Fact]
    public Task Then_ShortCircuitsOnFailure() => Arrange(() => new List<string>())
        .Act(async log =>
        {
            var result = await StartWithFailure("boom")
                .Then(v => { log.Add("invoked"); return Task.FromResult(Result<int, string>.Success(v)); });
            return (result, log);
        })
        .Assert(r =>
        {
            Assert.Equal("boom", r.result.FailureValue);
            Assert.Empty(r.log);
        });

    [Fact]
    public Task Then_OnFailure_TidiesUpAndCanMutateTheFailure() => Arrange(() => new List<string>())
        .Act(async log =>
        {
            var result = await StartWith(1)
                .Then(
                    v => Task.FromResult(Result<int, string>.Failure("boom")),
                    (s, f) => { log.Add($"cleanup:{s}"); return Task.FromResult(Result<int, string>.Failure($"{f}!")); });
            return (result, log);
        })
        .Assert(r =>
        {
            Assert.Equal("boom!", r.result.FailureValue);
            Assert.Equal(new[] { "cleanup:1" }, r.log);
        });

    [Fact]
    public Task Then_OnFailure_CannotTurnAFailureIntoASuccess() => Arrange(() => 1)
        .Act(x => StartWith(x)
            .Then(
                v => Task.FromResult(Result<int, string>.Failure("boom")),
                (s, f) => Task.FromResult(Result<int, string>.Success(99))))
        .Assert(r => Assert.Equal("boom", r.FailureValue));

    [Fact]
    public Task Then_OnFailure_IsNotInvokedOnSuccess() => Arrange(() => new List<string>())
        .Act(async log =>
        {
            var result = await StartWith(1)
                .Then(
                    v => Task.FromResult(Result<int, string>.Success(v + 1)),
                    (s, f) => { log.Add("cleanup"); return Task.FromResult(Result<int, string>.Failure(f)); });
            return (result, log);
        })
        .Assert(r =>
        {
            Assert.Equal(2, r.result.SuccessValue);
            Assert.Empty(r.log);
        });

    [Fact]
    public Task Then_SyncChainWithOnFailure() => Arrange(() => 1)
        .Act(x => x.ToSuccess<int, string>()
            .Then(
                v => Result<int, string>.Failure("boom"),
                (s, f) => Result<int, string>.Failure($"{f}:{s}")))
        .Assert(r => Assert.Equal("boom:1", r.FailureValue));

    [Fact]
    public Task IfThen_InvokesNextJobWhenConditionIsTrue() => Arrange(() => 2)
        .Act(x => StartWith(x)
            .IfThen(v => v % 2 == 0, v => Task.FromResult(Result<int, string>.Success(v * 10))))
        .Assert(r => Assert.Equal(20, r.SuccessValue));

    [Fact]
    public Task IfThen_SkipsNextJobWhenConditionIsFalse() => Arrange(() => 3)
        .Act(x => StartWith(x)
            .IfThen(v => v % 2 == 0, v => Task.FromResult(Result<int, string>.Success(v * 10))))
        .Assert(r => Assert.Equal(3, r.SuccessValue));

    [Fact]
    public Task IfThen_ShortCircuitsOnFailure() => Arrange(() => new List<string>())
        .Act(async log =>
        {
            var result = await StartWithFailure("boom")
                .IfThen(v => true, v => { log.Add("invoked"); return Task.FromResult(Result<int, string>.Success(v)); });
            return (result, log);
        })
        .Assert(r =>
        {
            Assert.Equal("boom", r.result.FailureValue);
            Assert.Empty(r.log);
        });

    [Fact]
    public Task IfThen_SyncChain() => Arrange(() => 2)
        .Act(x => x.ToSuccess<int, string>()
            .IfThen(v => v % 2 == 0, v => Result<int, string>.Success(v * 10))
            .IfThen(v => v % 2 == 1, v => Result<int, string>.Success(v + 1)))
        .Assert(r => Assert.Equal(20, r.SuccessValue));

    [Fact]
    public Task ThenForEach_InvokesTheJobForEachItem() => Arrange(() => 0)
        .Act(x => StartWith(x)
            .ThenForEach(
                v => new[] { 1, 2, 3 },
                (v, item) => Task.FromResult(Result<int, string>.Success(v + item))))
        .Assert(r => Assert.Equal(6, r.SuccessValue));

    [Fact]
    public Task ThenForEach_BreaksOnTheFirstFailure() => Arrange(() => new List<int>())
        .Act(async log =>
        {
            var result = await StartWith(0)
                .ThenForEach(
                    v => new[] { 1, 2, 3 },
                    (v, item) =>
                    {
                        log.Add(item);
                        return Task.FromResult(item == 2
                            ? Result<int, string>.Failure($"bad:{item}")
                            : Result<int, string>.Success(v + item));
                    });
            return (result, log);
        })
        .Assert(r =>
        {
            Assert.Equal("bad:2", r.result.FailureValue);
            Assert.Equal(new[] { 1, 2 }, r.log);
        });

    [Fact]
    public Task ThenForEach_OnFailure_ReceivesTheOriginalSuccess() => Arrange(() => 0)
        .Act(x => StartWith(x)
            .ThenForEach(
                v => new[] { 1, 2, 3 },
                (v, item) => Task.FromResult(item == 2
                    ? Result<int, string>.Failure("bad")
                    : Result<int, string>.Success(v + item)),
                (s, f) => Task.FromResult(Result<int, string>.Failure($"cleanup:{s}:{f}"))))
        .Assert(r => Assert.Equal("cleanup:0:bad", r.FailureValue));

    [Fact]
    public Task ToResult_ConvertsTheSuccessValue() => Arrange(() => 1)
        .Act(x => StartWith(x)
            .Then(v => Task.FromResult(Result<int, string>.Success(v + 1)))
            .ToResult(v => $"value:{v}"))
        .Assert(r => Assert.Equal("value:2", r.SuccessValue));

    [Fact]
    public Task ToResult_CascadesTheFailure() => Arrange(() => "boom")
        .Act(x => StartWithFailure(x).ToResult(v => $"value:{v}"))
        .Assert(r => Assert.Equal("boom", r.FailureValue));

    [Fact]
    public Task ToResult_SyncChain() => Arrange(() => 1)
        .Act(x => x.ToSuccess<int, string>().ToResult(v => $"value:{v}"))
        .Assert(r => Assert.Equal("value:1", r.SuccessValue));

    [Fact]
    public Task ThenWaitForAll_ReturnsTheOriginalSuccessWhenAllJobsSucceed() => Arrange(() => 1)
        .Act(x => StartWith(x)
            .ThenWaitForAll(
                v => Task.FromResult(Result<int, string>.Success(v + 1)),
                v => Task.FromResult(Result<int, string>.Success(v + 2))))
        .Assert(r => Assert.Equal(1, r.SuccessValue));

    [Fact]
    public Task ThenWaitForAll_ReturnsTheFirstFailure() => Arrange(() => 1)
        .Act(x => StartWith(x)
            .ThenWaitForAll(
                v => Task.FromResult(Result<int, string>.Success(v + 1)),
                v => Task.FromResult(Result<int, string>.Failure("boom")),
                v => Task.FromResult(Result<int, string>.Failure("late"))))
        .Assert(r => Assert.Equal("boom", r.FailureValue));

    [Fact]
    public Task ThenWaitForAll_UsesTheResultMergingStrategy() => Arrange(() => 1)
        .Act(x => StartWith(x)
            .ThenWaitForAll(
                (original, results) => Result<int, string>.Success(original + results.Sum(r => r.SuccessValue)),
                v => Task.FromResult(Result<int, string>.Success(v + 1)),
                v => Task.FromResult(Result<int, string>.Success(v + 2))))
        .Assert(r => Assert.Equal(6, r.SuccessValue));

    [Fact]
    public Task ThenWaitForAll_PassesThroughWhenNoJobsAreProvided() => Arrange(() => 1)
        .Act(x => StartWith(x).ThenWaitForAll())
        .Assert(r => Assert.Equal(1, r.SuccessValue));

    [Fact]
    public Task ThenWaitForFirst_ReturnsTheFirstCompletedResult() => Arrange(() => 1)
        .Act(x => StartWith(x)
            .ThenWaitForFirst(
                v => Task.FromResult(Result<int, string>.Success(v + 1)),
                async v => { await Task.Delay(5000); return Result<int, string>.Success(v + 2); }))
        .Assert(r => Assert.Equal(2, r.SuccessValue));

    [Fact]
    public Task ThenWaitForFirst_ShortCircuitsOnFailure() => Arrange(() => new List<string>())
        .Act(async log =>
        {
            var result = await StartWithFailure("boom")
                .ThenWaitForFirst(v => { log.Add("invoked"); return Task.FromResult(Result<int, string>.Success(v)); });
            return (result, log);
        })
        .Assert(r =>
        {
            Assert.Equal("boom", r.result.FailureValue);
            Assert.Empty(r.log);
        });

    [Fact]
    public Task ThenWaitForFirst_ReturnsWinnerEvenWhenALaterJobFaults() => Arrange(() => 1)
        .Act(x => StartWith(x)
            .ThenWaitForFirst(
                v => Task.FromResult(Result<int, string>.Success(v + 1)),
                async v => { await Task.Delay(50); throw new InvalidOperationException("late boom"); }))
        .Assert(r => Assert.Equal(2, r.SuccessValue));

    [Fact]
    public async Task ThenWaitForFirst_ObservesLoserFaultEvenWhenWinnerFaults()
    {
        // The winning job faults immediately, so awaiting it throws; a losing job faults shortly after.
        // ThenWaitForFirst wires the loser-observation continuation before awaiting the winner, so the
        // loser fault is observed rather than escaping as an UnobservedTaskException. Assert that
        // directly: capture the process-wide event (filtered to the loser's message so parallel tests
        // cannot pollute it), then force any would-be-unobserved task through finalization. If the fault
        // were left unobserved, its TaskExceptionHolder finalizer would raise the event during collection.
        var loserUnobserved = new List<Exception>();
        void OnUnobserved(object? _, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Exception is { } agg && agg.Flatten().InnerExceptions.Any(x => x.Message == "loser boom"))
            {
                lock (loserUnobserved) loserUnobserved.Add(agg);
                e.SetObserved();
            }
        }

        TaskScheduler.UnobservedTaskException += OnUnobserved;
        try
        {
            var loserFaulted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                StartWith(1).ThenWaitForFirst(
                    v => Task.FromException<Result<int, string>>(new InvalidOperationException("winner boom")),
                    async v =>
                    {
                        try { await Task.Delay(50); throw new InvalidOperationException("loser boom"); }
                        finally { loserFaulted.TrySetResult(); }
                    }));

            Assert.Equal("winner boom", ex.Message);
            await loserFaulted.Task.WaitAsync(TimeSpan.FromSeconds(10));

            for (var i = 0; i < 5; i++)
            {
                await Task.Delay(50);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            GC.Collect();

            Assert.Empty(loserUnobserved);
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= OnUnobserved;
        }
    }

    [Fact]
    public async Task Then_WithCancellation_ThrowsWhenAlreadyCancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            StartWith(1).Then((v, ct) => Task.FromResult(Result<int, string>.Success(v + 1)), cts.Token));
    }

    [Fact]
    public async Task Then_WithCancellation_AFailureWinsOverCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await StartWithFailure("boom")
            .Then((v, ct) => Task.FromResult(Result<int, string>.Success(v + 1)), cts.Token);

        Assert.Equal("boom", result.FailureValue);
    }

    [Fact]
    public async Task Then_WithCancellation_PassesTheTokenToTheNextJob()
    {
        using var cts = new CancellationTokenSource();

        var result = await StartWith(1)
            .Then((v, ct) => Task.FromResult(Result<int, string>.Success(ct == cts.Token ? v + 1 : v)), cts.Token);

        Assert.Equal(2, result.SuccessValue);
    }

    [Fact]
    public async Task Then_WithCancellation_CanCancelGracefully()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await StartWith(1)
            .Then(
                (v, ct) => Task.FromResult(Result<int, string>.Success(ct.IsCancellationRequested ? v + 100 : v)),
                cts.Token, throwOnCancellation: false);

        Assert.Equal(101, result.SuccessValue);
    }

    [Fact]
    public async Task Then_WithCancellation_OnFailure_TidiesUpAndCanMutateTheFailure()
    {
        var result = await StartWith(1)
            .Then(
                (v, ct) => Task.FromResult(Result<int, string>.Failure("boom")),
                (s, f, ct) => Task.FromResult(Result<int, string>.Failure($"{f}:{s}")),
                CancellationToken.None);

        Assert.Equal("boom:1", result.FailureValue);
    }

    [Fact]
    public async Task IfThen_WithCancellation_InvokesNextJobWhenConditionIsTrue()
    {
        var result = await StartWith(2)
            .IfThen((v, ct) => v % 2 == 0, (v, ct) => Task.FromResult(Result<int, string>.Success(v * 10)), CancellationToken.None);

        Assert.Equal(20, result.SuccessValue);
    }

    [Fact]
    public async Task ThenForEach_WithCancellation_ThrowsBetweenIterations()
    {
        using var cts = new CancellationTokenSource();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            StartWith(0).ThenForEach(
                v => new[] { 1, 2, 3 },
                (v, item, ct) =>
                {
                    cts.Cancel();
                    return Task.FromResult(Result<int, string>.Success(v + item));
                },
                cts.Token));
    }

    [Fact]
    public async Task ToResult_WithCancellation_ThrowsWhenAlreadyCancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            StartWith(1).ToResult(v => $"value:{v}", cts.Token));
    }

    [Fact]
    public async Task ThenWaitForAll_WithCancellation_PassesTheTokenToAllJobs()
    {
        using var cts = new CancellationTokenSource();

        var result = await StartWith(1)
            .ThenWaitForAll(
                cts.Token, true,
                (v, ct) => Task.FromResult(Result<int, string>.Success(ct == cts.Token ? v : -1)),
                (v, ct) => Task.FromResult(Result<int, string>.Success(ct == cts.Token ? v : -1)));

        Assert.Equal(1, result.SuccessValue);
    }

    [Fact]
    public async Task ThenWaitForFirst_WithCancellation_CancelsTheRemainingJobs()
    {
        var remainingJobCancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var result = await StartWith(1)
            .ThenWaitForFirst(
                CancellationToken.None, true,
                (v, ct) => Task.FromResult(Result<int, string>.Success(v + 1)),
                async (v, ct) =>
                {
                    try { await Task.Delay(Timeout.Infinite, ct); }
                    catch (OperationCanceledException) { remainingJobCancelled.SetResult(true); }
                    return Result<int, string>.Success(v + 2);
                });

        Assert.Equal(2, result.SuccessValue);
        Assert.True(await remainingJobCancelled.Task.WaitAsync(TimeSpan.FromSeconds(10)));
    }
}
