namespace PipeEx.ResultChaining;

/// <summary>
/// A set of extension methods which allow chaining of methods in a fluent, english sentence like chain or flow, with cancellation support.
/// In order to be chained a method need only return a Task of <see cref="Result{TSuccess, TFailure}"/>,
/// where TSuccess == Success and TFailure == Failure.
/// </summary>
public static class ResultChainingWithCancellation
{
    /// <summary>
    /// Chains the next asynchronous job onto the result of the previous one. Supports cancellation.
    /// The result of the <paramref name="source"/> Task is evaluated and if it contains a failure, that failure is
    /// immediately returned and <paramref name="nextJob"/> is not invoked. Otherwise the success value is passed to <paramref name="nextJob"/>.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The resulting Task of the previous link in the chain.</param>
    /// <param name="nextJob">A func containing the next piece of work in the chain.</param>
    /// <param name="ct">A token which enables cancellation, checked immediately and also passed into the lambda to enable checking for cancellation at more granular levels.</param>
    /// <param name="throwOnCancellation">Flag which can be used to disable calling of ThrowIfCancellationRequested(), useful for cancelling gracefully while still returning some result etc.</param>
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static async Task<Result<TSuccess, TFailure>> Then<TSuccess, TFailure>(
        this Task<Result<TSuccess, TFailure>> source,
        Func<TSuccess, CancellationToken, Task<Result<TSuccess, TFailure>>> nextJob,
        CancellationToken ct, bool throwOnCancellation = true)
    {
        var successOrFailure = await source.ConfigureAwait(false);
        if (successOrFailure.IsFailure)
            return successOrFailure;

        if (throwOnCancellation)
            ct.ThrowIfCancellationRequested();

        return await nextJob(successOrFailure.SuccessValue, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Chains the next asynchronous job onto the result of the previous one. Supports cancellation.
    /// The result of the <paramref name="source"/> Task is evaluated and if it contains a failure, that failure is
    /// immediately returned and <paramref name="nextJob"/> is not invoked. Otherwise the success value is passed to <paramref name="nextJob"/>.
    /// The <paramref name="onFailure"/> func enables tidying up tasks to be performed if <paramref name="nextJob"/> fails;
    /// it may mutate the failure but cannot turn it into a success.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The resulting Task of the previous link in the chain.</param>
    /// <param name="nextJob">A func containing the next piece of work in the chain.</param>
    /// <param name="onFailure">A func which will be invoked if <paramref name="nextJob"/> fails, it is passed the success value and the failure
    /// and should perform any tidying up tasks as a result of the failure, before returning the final failure to be passed down the chain.</param>
    /// <param name="ct">A token which enables cancellation, checked immediately and also passed into the lambda to enable checking for cancellation at more granular levels.</param>
    /// <param name="throwOnCancellation">Flag which can be used to disable calling of ThrowIfCancellationRequested(), useful for cancelling gracefully while still returning some result etc.</param>
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static async Task<Result<TSuccess, TFailure>> Then<TSuccess, TFailure>(
        this Task<Result<TSuccess, TFailure>> source,
        Func<TSuccess, CancellationToken, Task<Result<TSuccess, TFailure>>> nextJob,
        Func<TSuccess, TFailure, CancellationToken, Task<Result<TSuccess, TFailure>>> onFailure,
        CancellationToken ct, bool throwOnCancellation = true)
    {
        var successOrFailure = await source.ConfigureAwait(false);
        if (successOrFailure.IsFailure)
            return successOrFailure;

        if (throwOnCancellation)
            ct.ThrowIfCancellationRequested();

        var currentSuccess = successOrFailure.SuccessValue;
        var result = await nextJob(currentSuccess, ct).ConfigureAwait(false);

        if (result.IsSuccess)
            return result;

        var finalFailure = await onFailure(currentSuccess, result.FailureValue, ct).ConfigureAwait(false);

        return finalFailure.IsFailure ? finalFailure : result.FailureValue;
    }

    /// <summary>
    /// Chains the next asynchronous job onto the result of the previous one, but only invokes <paramref name="nextJob"/>
    /// if the <paramref name="condition"/> func evaluates to true. Supports cancellation.
    /// The result of the <paramref name="source"/> Task is evaluated and if it contains a failure, that failure is immediately returned.
    /// If the <paramref name="condition"/> func evaluates to false, the current success value is passed down the chain unchanged.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The resulting Task of the previous link in the chain.</param>
    /// <param name="condition">This func will be invoked first, only if true is returned will <paramref name="nextJob"/> be invoked,
    /// otherwise the current success value will be passed to the next link in the chain.</param>
    /// <param name="nextJob">A func containing the next piece of work in the chain.</param>
    /// <param name="ct">A token which enables cancellation, checked immediately and also passed into the lambda to enable checking for cancellation at more granular levels.</param>
    /// <param name="throwOnCancellation">Flag which can be used to disable calling of ThrowIfCancellationRequested(), useful for cancelling gracefully while still returning some result etc.</param>
    /// <param name="onFailure">A func which will be invoked if <paramref name="nextJob"/> fails, it is passed the success value and the failure
    /// and should perform any tidying up tasks as a result of the failure, before returning the final failure to be passed down the chain.</param>
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static async Task<Result<TSuccess, TFailure>> IfThen<TSuccess, TFailure>(
        this Task<Result<TSuccess, TFailure>> source,
        Func<TSuccess, CancellationToken, bool> condition,
        Func<TSuccess, CancellationToken, Task<Result<TSuccess, TFailure>>> nextJob,
        CancellationToken ct, bool throwOnCancellation = true,
        Func<TSuccess, TFailure, CancellationToken, Task<Result<TSuccess, TFailure>>>? onFailure = null)
    {
        var successOrFailure = await source.ConfigureAwait(false);
        if (successOrFailure.IsFailure)
            return successOrFailure;

        if (throwOnCancellation)
            ct.ThrowIfCancellationRequested();

        var currentSuccess = successOrFailure.SuccessValue;
        if (condition(currentSuccess, ct) is false)
            return successOrFailure;

        var result = await nextJob(currentSuccess, ct).ConfigureAwait(false);

        if (result.IsSuccess)
            return result;

        if (onFailure is null)
            return result;

        var finalFailure = await onFailure(currentSuccess, result.FailureValue, ct).ConfigureAwait(false);

        return finalFailure.IsFailure ? finalFailure : result.FailureValue;
    }

    /// <summary>
    /// Chains a job which is executed once per item produced by <paramref name="itemsToIterateOver"/>. Supports cancellation.
    /// The result of the <paramref name="source"/> Task is evaluated and if it contains a failure, that failure is immediately returned.
    /// Each successful result is passed to the task for the next item; the loop breaks on the first failure.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <typeparam name="TItem">The type to be iterated over.</typeparam>
    /// <param name="source">The resulting Task of the previous link in the chain.</param>
    /// <param name="itemsToIterateOver">A func which should produce an IEnumerable of <typeparamref name="TItem"/>.</param>
    /// <param name="taskForEachItem">A func which will be called for each <typeparamref name="TItem"/> and should return a Task of <see cref="Result{TSuccess, TFailure}"/>.</param>
    /// <param name="ct">A token which enables cancellation, checked before each iteration and also passed into the lambda to enable checking for cancellation at more granular levels.</param>
    /// <param name="throwOnCancellation">Flag which can be used to disable calling of ThrowIfCancellationRequested(), useful for cancelling gracefully while still returning some result etc.</param>
    /// <param name="onFailure">A func which will be invoked if any iteration fails, it is passed the success value and the failure
    /// and should perform any tidying up tasks as a result of the failure, before returning the final failure to be passed down the chain.</param>
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static async Task<Result<TSuccess, TFailure>> ThenForEach<TSuccess, TFailure, TItem>(
        this Task<Result<TSuccess, TFailure>> source,
        Func<TSuccess, IEnumerable<TItem>> itemsToIterateOver,
        Func<TSuccess, TItem, CancellationToken, Task<Result<TSuccess, TFailure>>> taskForEachItem,
        CancellationToken ct, bool throwOnCancellation = true,
        Func<TSuccess, TFailure, CancellationToken, Task<Result<TSuccess, TFailure>>>? onFailure = null)
    {
        var successOrFailure = await source.ConfigureAwait(false);
        if (successOrFailure.IsFailure)
            return successOrFailure;

        var currentSuccess = successOrFailure.SuccessValue;
        var items = itemsToIterateOver(currentSuccess);

        var itemResult = successOrFailure;
        foreach (var item in items)
        {
            if (throwOnCancellation)
                ct.ThrowIfCancellationRequested();

            itemResult = await taskForEachItem(itemResult.SuccessValue, item, ct).ConfigureAwait(false);

            if (itemResult.IsFailure)
                break;
        }

        if (itemResult.IsSuccess)
            return itemResult;

        if (onFailure is null)
            return itemResult;

        var finalFailure = await onFailure(currentSuccess, itemResult.FailureValue, ct).ConfigureAwait(false);

        return finalFailure.IsFailure ? finalFailure : itemResult.FailureValue;
    }

    /// <summary>
    /// Converts the success value at the end of an asynchronous chain to a new type <typeparamref name="TResult"/>,
    /// if all operations in the chain have been successful. A failure cascades through unchanged. Supports cancellation.
    /// </summary>
    /// <typeparam name="TResult">The new type to convert the success value into.</typeparam>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The resulting Task of the previous link in the chain.</param>
    /// <param name="convertToResult">A func provided to do the conversion.</param>
    /// <param name="ct">A token which enables cancellation, checked before the conversion is invoked.</param>
    /// <param name="throwOnCancellation">Flag which can be used to disable calling of ThrowIfCancellationRequested(), useful for cancelling gracefully while still returning some result etc.</param>
    /// <returns>A Task of <see cref="Result{TResult, TFailure}"/>, where the success value has been converted.</returns>
    public static async Task<Result<TResult, TFailure>> ToResult<TResult, TSuccess, TFailure>(
        this Task<Result<TSuccess, TFailure>> source,
        Func<TSuccess, TResult> convertToResult,
        CancellationToken ct, bool throwOnCancellation = true)
    {
        var successOrFailure = await source.ConfigureAwait(false);
        if (successOrFailure.IsFailure)
            return successOrFailure.FailureValue;

        if (throwOnCancellation)
            ct.ThrowIfCancellationRequested();

        return convertToResult(successOrFailure.SuccessValue);
    }

    /// <summary>
    /// Chains an array of jobs which will be executed in parallel. This method will return once all jobs have completed. Supports cancellation.
    /// The result of the <paramref name="source"/> Task is evaluated and if it contains a failure, that failure is immediately returned.
    /// Please note, the success value is passed into each job by ref, so care must be taken around any mutation of any state on it.
    /// This library cannot know how to merge the result of each job, so a <paramref name="resultMergingStrategy"/> must be provided.
    /// Another overload of this method provides a naive default merging strategy.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The resulting Task of the previous link in the chain.</param>
    /// <param name="resultMergingStrategy">A func which is passed the original success value and a list of results from the jobs,
    /// it should decide how to merge the results once they have all returned i.e. what to return from this method.</param>
    /// <param name="ct">A token which enables cancellation, checked immediately and also passed into the lambda to enable checking for cancellation at more granular levels.</param>
    /// <param name="throwOnCancellation">Flag which can be used to disable calling of ThrowIfCancellationRequested(), useful for cancelling gracefully while still returning some result etc.</param>
    /// <param name="tasks">A list of jobs to execute in parallel.</param>
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static async Task<Result<TSuccess, TFailure>> ThenWaitForAll<TSuccess, TFailure>(
        this Task<Result<TSuccess, TFailure>> source,
        Func<TSuccess, CancellationToken, List<Result<TSuccess, TFailure>>, Result<TSuccess, TFailure>> resultMergingStrategy,
        CancellationToken ct, bool throwOnCancellation = true,
        params Func<TSuccess, CancellationToken, Task<Result<TSuccess, TFailure>>>[] tasks)
    {
        var successOrFailure = await source.ConfigureAwait(false);
        if (successOrFailure.IsFailure)
            return successOrFailure;

        if (throwOnCancellation)
            ct.ThrowIfCancellationRequested();

        if (tasks.Length == 0)
            return successOrFailure;

        var currentSuccess = successOrFailure.SuccessValue;
        var taskResults = await Task.WhenAll(tasks.Select(task => task(currentSuccess, ct))).ConfigureAwait(false);

        return resultMergingStrategy(currentSuccess, ct, taskResults.ToList());
    }

    /// <summary>
    /// Chains an array of jobs which will be executed in parallel. This method will return once all jobs have completed. Supports cancellation.
    /// The result of the <paramref name="source"/> Task is evaluated and if it contains a failure, that failure is immediately returned.
    /// Please note, the success value is passed into each job by ref, so care must be taken around any mutation of any state on it.
    /// The naive default result merging strategy is to return the first failure if any jobs return a failure,
    /// otherwise return the original success value passed into this method.
    /// A better strategy can <i>and should</i> be provided using the overload of this method.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The resulting Task of the previous link in the chain.</param>
    /// <param name="ct">A token which enables cancellation, checked immediately and also passed into the lambda to enable checking for cancellation at more granular levels.</param>
    /// <param name="throwOnCancellation">Flag which can be used to disable calling of ThrowIfCancellationRequested(), useful for cancelling gracefully while still returning some result etc.</param>
    /// <param name="tasks">A list of jobs to execute in parallel.</param>
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static async Task<Result<TSuccess, TFailure>> ThenWaitForAll<TSuccess, TFailure>(
        this Task<Result<TSuccess, TFailure>> source,
        CancellationToken ct, bool throwOnCancellation = true,
        params Func<TSuccess, CancellationToken, Task<Result<TSuccess, TFailure>>>[] tasks)
    {
        static Result<TSuccess, TFailure> DefaultResultMergingStrategy(TSuccess input, CancellationToken ct, List<Result<TSuccess, TFailure>> results)
        {
            return results.Any(x => x.IsFailure) ? results.First(x => x.IsFailure) : input;
        }

        return await source.ThenWaitForAll(DefaultResultMergingStrategy, ct, throwOnCancellation, tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Chains an array of jobs which will be executed in parallel. This method will return immediately once the first job has completed. Supports cancellation.
    /// The result of the <paramref name="source"/> Task is evaluated and if it contains a failure, that failure is immediately returned.
    /// The result of the first completed job will be returned and cancellation will be signalled to the remaining jobs.
    /// Please note, the success value is passed into each job by ref, so care must be taken around any mutation of any state on it.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The resulting Task of the previous link in the chain.</param>
    /// <param name="ct">A token which enables cancellation, checked immediately and also passed into the lambda to enable checking for cancellation at more granular levels.</param>
    /// <param name="throwOnCancellation">Flag which can be used to disable calling of ThrowIfCancellationRequested(), useful for cancelling gracefully while still returning some result etc.</param>
    /// <param name="tasks">A list of jobs to execute in parallel.</param>
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static async Task<Result<TSuccess, TFailure>> ThenWaitForFirst<TSuccess, TFailure>(
        this Task<Result<TSuccess, TFailure>> source,
        CancellationToken ct, bool throwOnCancellation = true,
        params Func<TSuccess, CancellationToken, Task<Result<TSuccess, TFailure>>>[] tasks)
    {
        var successOrFailure = await source.ConfigureAwait(false);
        if (successOrFailure.IsFailure)
            return successOrFailure;

        if (throwOnCancellation)
            ct.ThrowIfCancellationRequested();

        if (tasks.Length == 0)
            return successOrFailure;

        var remainingTasksCanceller = CancellationTokenSource.CreateLinkedTokenSource(ct);

        var currentSuccess = successOrFailure.SuccessValue;
        var runningTasks = new List<Task<Result<TSuccess, TFailure>>>(tasks.Length);
        try
        {
            foreach (var task in tasks)
                runningTasks.Add(task(currentSuccess, remainingTasksCanceller.Token));
        }
        catch
        {
            // A factory that throws synchronously must not abandon the jobs already started: signal
            // them to stop, observe their outcomes, and dispose the linked source once they finish.
            remainingTasksCanceller.Cancel();
            _ = Task.WhenAll(runningTasks).ContinueWith(
                t => { _ = t.Exception; remainingTasksCanceller.Dispose(); },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
            throw;
        }
        var winner = await Task.WhenAny(runningTasks).ConfigureAwait(false);

        // Signal cancellation to the losing jobs as soon as the first job completes, even if the
        // winning job faulted or cancelled (awaiting it below may throw before we would otherwise reach this).
        remainingTasksCanceller.Cancel();

        // The linked source must outlive the remaining jobs, dispose it (and observe their exceptions) once they have finished.
        _ = Task.WhenAll(runningTasks).ContinueWith(
            t => { _ = t.Exception; remainingTasksCanceller.Dispose(); },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

        return await winner.ConfigureAwait(false);
    }
}
