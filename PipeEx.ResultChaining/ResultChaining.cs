namespace PipeEx.ResultChaining;

/// <summary>
/// A set of extension methods which allow chaining of methods in a fluent, english sentence like chain or flow.
/// In order to be chained a method need only return a <see cref="Result{TSuccess, TFailure}"/> or a
/// Task of <see cref="Result{TSuccess, TFailure}"/>, where TSuccess == Success and TFailure == Failure.
/// </summary>
public static class ResultChaining
{
    /// <summary>
    /// Wraps a value as a successful <see cref="Result{TSuccess, TFailure}"/>, useful for starting a chain.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The source object.</param>
    /// <returns>A successful <see cref="Result{TSuccess, TFailure}"/> wrapping the source object.</returns>
    public static Result<TSuccess, TFailure> ToSuccess<TSuccess, TFailure>(this TSuccess source) =>
        Result<TSuccess, TFailure>.Success(source);

    /// <summary>
    /// Wraps a value as a failed <see cref="Result{TSuccess, TFailure}"/>.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The failure object.</param>
    /// <returns>A failed <see cref="Result{TSuccess, TFailure}"/> wrapping the source object.</returns>
    public static Result<TSuccess, TFailure> ToFailure<TSuccess, TFailure>(this TFailure source) =>
        Result<TSuccess, TFailure>.Failure(source);

    /// <summary>
    /// Chains the next job onto the result of the previous one.
    /// If the <paramref name="source"/> contains a failure, that failure is immediately returned and
    /// <paramref name="nextJob"/> is not invoked. Otherwise the success value is passed to <paramref name="nextJob"/>.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The result of the previous link in the chain.</param>
    /// <param name="nextJob">A func containing the next piece of work in the chain.</param>
    /// <returns>A <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static Result<TSuccess, TFailure> Then<TSuccess, TFailure>(
        this Result<TSuccess, TFailure> source,
        Func<TSuccess, Result<TSuccess, TFailure>> nextJob)
    {
        if (source.IsFailure)
            return source;

        return nextJob(source.SuccessValue);
    }

    /// <summary>
    /// Chains the next job onto the result of the previous one.
    /// If the <paramref name="source"/> contains a failure, that failure is immediately returned and
    /// <paramref name="nextJob"/> is not invoked. Otherwise the success value is passed to <paramref name="nextJob"/>.
    /// The <paramref name="onFailure"/> func enables tidying up tasks to be performed if <paramref name="nextJob"/> fails;
    /// it may mutate the failure but cannot turn it into a success.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The result of the previous link in the chain.</param>
    /// <param name="nextJob">A func containing the next piece of work in the chain.</param>
    /// <param name="onFailure">A func which will be invoked if <paramref name="nextJob"/> fails, it is passed the success value and the failure
    /// and should perform any tidying up tasks as a result of the failure, before returning the final failure to be passed down the chain.</param>
    /// <returns>A <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static Result<TSuccess, TFailure> Then<TSuccess, TFailure>(
        this Result<TSuccess, TFailure> source,
        Func<TSuccess, Result<TSuccess, TFailure>> nextJob,
        Func<TSuccess, TFailure, Result<TSuccess, TFailure>> onFailure)
    {
        if (source.IsFailure)
            return source;

        var currentSuccess = source.SuccessValue;
        var result = nextJob(currentSuccess);

        if (result.IsSuccess)
            return result;

        var finalFailure = onFailure(currentSuccess, result.FailureValue);

        return finalFailure.IsFailure ? finalFailure : result.FailureValue;
    }

    /// <summary>
    /// Chains the next asynchronous job onto the result of the previous one.
    /// If the <paramref name="source"/> contains a failure, that failure is immediately returned and
    /// <paramref name="nextJob"/> is not invoked. Otherwise the success value is passed to <paramref name="nextJob"/>.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The result of the previous link in the chain.</param>
    /// <param name="nextJob">A func containing the next piece of work in the chain.</param>
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static Task<Result<TSuccess, TFailure>> Then<TSuccess, TFailure>(
        this Result<TSuccess, TFailure> source,
        Func<TSuccess, Task<Result<TSuccess, TFailure>>> nextJob)
    {
        if (source.IsFailure)
            return Task.FromResult(source);

        return nextJob(source.SuccessValue);
    }

    /// <summary>
    /// Chains the next asynchronous job onto the result of the previous one.
    /// If the <paramref name="source"/> contains a failure, that failure is immediately returned and
    /// <paramref name="nextJob"/> is not invoked. Otherwise the success value is passed to <paramref name="nextJob"/>.
    /// The <paramref name="onFailure"/> func enables tidying up tasks to be performed if <paramref name="nextJob"/> fails;
    /// it may mutate the failure but cannot turn it into a success.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The result of the previous link in the chain.</param>
    /// <param name="nextJob">A func containing the next piece of work in the chain.</param>
    /// <param name="onFailure">A func which will be invoked if <paramref name="nextJob"/> fails, it is passed the success value and the failure
    /// and should perform any tidying up tasks as a result of the failure, before returning the final failure to be passed down the chain.</param>
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static Task<Result<TSuccess, TFailure>> Then<TSuccess, TFailure>(
        this Result<TSuccess, TFailure> source,
        Func<TSuccess, Task<Result<TSuccess, TFailure>>> nextJob,
        Func<TSuccess, TFailure, Task<Result<TSuccess, TFailure>>> onFailure) =>
        Task.FromResult(source).Then(nextJob, onFailure);

    /// <summary>
    /// Chains the next job onto the result of the previous asynchronous one.
    /// The result of the <paramref name="source"/> Task is evaluated and if it contains a failure, that failure is
    /// immediately returned and <paramref name="nextJob"/> is not invoked. Otherwise the success value is passed to <paramref name="nextJob"/>.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The resulting Task of the previous link in the chain.</param>
    /// <param name="nextJob">A func containing the next piece of work in the chain.</param>
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static async Task<Result<TSuccess, TFailure>> Then<TSuccess, TFailure>(
        this Task<Result<TSuccess, TFailure>> source,
        Func<TSuccess, Result<TSuccess, TFailure>> nextJob)
    {
        var successOrFailure = await source.ConfigureAwait(false);
        if (successOrFailure.IsFailure)
            return successOrFailure;

        return nextJob(successOrFailure.SuccessValue);
    }

    /// <summary>
    /// Chains the next job onto the result of the previous asynchronous one.
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
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static async Task<Result<TSuccess, TFailure>> Then<TSuccess, TFailure>(
        this Task<Result<TSuccess, TFailure>> source,
        Func<TSuccess, Result<TSuccess, TFailure>> nextJob,
        Func<TSuccess, TFailure, Result<TSuccess, TFailure>> onFailure)
    {
        var successOrFailure = await source.ConfigureAwait(false);
        return successOrFailure.Then(nextJob, onFailure);
    }

    /// <summary>
    /// Chains the next asynchronous job onto the result of the previous one.
    /// The result of the <paramref name="source"/> Task is evaluated and if it contains a failure, that failure is
    /// immediately returned and <paramref name="nextJob"/> is not invoked. Otherwise the success value is passed to <paramref name="nextJob"/>.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The resulting Task of the previous link in the chain.</param>
    /// <param name="nextJob">A func containing the next piece of work in the chain.</param>
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static async Task<Result<TSuccess, TFailure>> Then<TSuccess, TFailure>(
        this Task<Result<TSuccess, TFailure>> source,
        Func<TSuccess, Task<Result<TSuccess, TFailure>>> nextJob)
    {
        var successOrFailure = await source.ConfigureAwait(false);
        if (successOrFailure.IsFailure)
            return successOrFailure;

        return await nextJob(successOrFailure.SuccessValue).ConfigureAwait(false);
    }

    /// <summary>
    /// Chains the next asynchronous job onto the result of the previous one.
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
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static async Task<Result<TSuccess, TFailure>> Then<TSuccess, TFailure>(
        this Task<Result<TSuccess, TFailure>> source,
        Func<TSuccess, Task<Result<TSuccess, TFailure>>> nextJob,
        Func<TSuccess, TFailure, Task<Result<TSuccess, TFailure>>> onFailure)
    {
        var successOrFailure = await source.ConfigureAwait(false);
        if (successOrFailure.IsFailure)
            return successOrFailure;

        var currentSuccess = successOrFailure.SuccessValue;
        var result = await nextJob(currentSuccess).ConfigureAwait(false);

        if (result.IsSuccess)
            return result;

        var finalFailure = await onFailure(currentSuccess, result.FailureValue).ConfigureAwait(false);

        return finalFailure.IsFailure ? finalFailure : result.FailureValue;
    }

    /// <summary>
    /// Chains the next job onto the result of the previous one, but only invokes <paramref name="nextJob"/>
    /// if the <paramref name="condition"/> func evaluates to true.
    /// If the <paramref name="source"/> contains a failure, that failure is immediately returned.
    /// If the <paramref name="condition"/> func evaluates to false, the current success value is passed down the chain unchanged.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The result of the previous link in the chain.</param>
    /// <param name="condition">This func will be invoked first, only if true is returned will <paramref name="nextJob"/> be invoked,
    /// otherwise the current success value will be passed to the next link in the chain.</param>
    /// <param name="nextJob">A func containing the next piece of work in the chain.</param>
    /// <param name="onFailure">A func which will be invoked if <paramref name="nextJob"/> fails, it is passed the success value and the failure
    /// and should perform any tidying up tasks as a result of the failure, before returning the final failure to be passed down the chain.</param>
    /// <returns>A <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static Result<TSuccess, TFailure> IfThen<TSuccess, TFailure>(
        this Result<TSuccess, TFailure> source,
        Func<TSuccess, bool> condition,
        Func<TSuccess, Result<TSuccess, TFailure>> nextJob,
        Func<TSuccess, TFailure, Result<TSuccess, TFailure>>? onFailure = null)
    {
        if (source.IsFailure)
            return source;

        if (condition(source.SuccessValue) is false)
            return source;

        return onFailure is null ? source.Then(nextJob) : source.Then(nextJob, onFailure);
    }

    /// <summary>
    /// Chains the next asynchronous job onto the result of the previous one, but only invokes <paramref name="nextJob"/>
    /// if the <paramref name="condition"/> func evaluates to true.
    /// The result of the <paramref name="source"/> Task is evaluated and if it contains a failure, that failure is immediately returned.
    /// If the <paramref name="condition"/> func evaluates to false, the current success value is passed down the chain unchanged.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The resulting Task of the previous link in the chain.</param>
    /// <param name="condition">This func will be invoked first, only if true is returned will <paramref name="nextJob"/> be invoked,
    /// otherwise the current success value will be passed to the next link in the chain.</param>
    /// <param name="nextJob">A func containing the next piece of work in the chain.</param>
    /// <param name="onFailure">A func which will be invoked if <paramref name="nextJob"/> fails, it is passed the success value and the failure
    /// and should perform any tidying up tasks as a result of the failure, before returning the final failure to be passed down the chain.</param>
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static async Task<Result<TSuccess, TFailure>> IfThen<TSuccess, TFailure>(
        this Task<Result<TSuccess, TFailure>> source,
        Func<TSuccess, bool> condition,
        Func<TSuccess, Task<Result<TSuccess, TFailure>>> nextJob,
        Func<TSuccess, TFailure, Task<Result<TSuccess, TFailure>>>? onFailure = null)
    {
        var successOrFailure = await source.ConfigureAwait(false);
        if (successOrFailure.IsFailure)
            return successOrFailure;

        var currentSuccess = successOrFailure.SuccessValue;
        if (condition(currentSuccess) is false)
            return successOrFailure;

        var result = await nextJob(currentSuccess).ConfigureAwait(false);

        if (result.IsSuccess)
            return result;

        if (onFailure is null)
            return result;

        var finalFailure = await onFailure(currentSuccess, result.FailureValue).ConfigureAwait(false);

        return finalFailure.IsFailure ? finalFailure : result.FailureValue;
    }

    /// <summary>
    /// Chains a job which is executed once per item produced by <paramref name="itemsToIterateOver"/>.
    /// The result of the <paramref name="source"/> Task is evaluated and if it contains a failure, that failure is immediately returned.
    /// Each successful result is passed to the task for the next item; the loop breaks on the first failure.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <typeparam name="TItem">The type to be iterated over.</typeparam>
    /// <param name="source">The resulting Task of the previous link in the chain.</param>
    /// <param name="itemsToIterateOver">A func which should produce an IEnumerable of <typeparamref name="TItem"/>.</param>
    /// <param name="taskForEachItem">A func which will be called for each <typeparamref name="TItem"/> and should return a Task of <see cref="Result{TSuccess, TFailure}"/>.</param>
    /// <param name="onFailure">A func which will be invoked if any iteration fails, it is passed the success value and the failure
    /// and should perform any tidying up tasks as a result of the failure, before returning the final failure to be passed down the chain.</param>
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static async Task<Result<TSuccess, TFailure>> ThenForEach<TSuccess, TFailure, TItem>(
        this Task<Result<TSuccess, TFailure>> source,
        Func<TSuccess, IEnumerable<TItem>> itemsToIterateOver,
        Func<TSuccess, TItem, Task<Result<TSuccess, TFailure>>> taskForEachItem,
        Func<TSuccess, TFailure, Task<Result<TSuccess, TFailure>>>? onFailure = null)
    {
        var successOrFailure = await source.ConfigureAwait(false);
        if (successOrFailure.IsFailure)
            return successOrFailure;

        var currentSuccess = successOrFailure.SuccessValue;
        var items = itemsToIterateOver(currentSuccess);

        var itemResult = successOrFailure;
        foreach (var item in items)
        {
            itemResult = await taskForEachItem(itemResult.SuccessValue, item).ConfigureAwait(false);

            if (itemResult.IsFailure)
                break;
        }

        if (itemResult.IsSuccess)
            return itemResult;

        if (onFailure is null)
            return itemResult;

        var finalFailure = await onFailure(currentSuccess, itemResult.FailureValue).ConfigureAwait(false);

        return finalFailure.IsFailure ? finalFailure : itemResult.FailureValue;
    }

    /// <summary>
    /// Converts the success value at the end of a chain to a new type <typeparamref name="TResult"/>,
    /// if all operations in the chain have been successful. A failure cascades through unchanged.
    /// </summary>
    /// <typeparam name="TResult">The new type to convert the success value into.</typeparam>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The result of the previous link in the chain.</param>
    /// <param name="convertToResult">A func provided to do the conversion.</param>
    /// <returns>A <see cref="Result{TResult, TFailure}"/>, where the success value has been converted.</returns>
    public static Result<TResult, TFailure> ToResult<TResult, TSuccess, TFailure>(
        this Result<TSuccess, TFailure> source,
        Func<TSuccess, TResult> convertToResult) =>
        source.Match<Result<TResult, TFailure>>(
            success => convertToResult(success),
            failure => failure);

    /// <summary>
    /// Converts the success value at the end of an asynchronous chain to a new type <typeparamref name="TResult"/>,
    /// if all operations in the chain have been successful. A failure cascades through unchanged.
    /// </summary>
    /// <typeparam name="TResult">The new type to convert the success value into.</typeparam>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The resulting Task of the previous link in the chain.</param>
    /// <param name="convertToResult">A func provided to do the conversion.</param>
    /// <returns>A Task of <see cref="Result{TResult, TFailure}"/>, where the success value has been converted.</returns>
    public static async Task<Result<TResult, TFailure>> ToResult<TResult, TSuccess, TFailure>(
        this Task<Result<TSuccess, TFailure>> source,
        Func<TSuccess, TResult> convertToResult)
    {
        var successOrFailure = await source.ConfigureAwait(false);
        return successOrFailure.ToResult(convertToResult);
    }

    /// <summary>
    /// Chains an array of jobs which will be executed in parallel. This method will return once all jobs have completed.
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
    /// <param name="tasks">A list of jobs to execute in parallel.</param>
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static async Task<Result<TSuccess, TFailure>> ThenWaitForAll<TSuccess, TFailure>(
        this Task<Result<TSuccess, TFailure>> source,
        Func<TSuccess, List<Result<TSuccess, TFailure>>, Result<TSuccess, TFailure>> resultMergingStrategy,
        params Func<TSuccess, Task<Result<TSuccess, TFailure>>>[] tasks)
    {
        var successOrFailure = await source.ConfigureAwait(false);
        if (successOrFailure.IsFailure)
            return successOrFailure;

        if (tasks.Length == 0)
            return successOrFailure;

        var currentSuccess = successOrFailure.SuccessValue;
        var taskResults = await Task.WhenAll(tasks.Select(task => task(currentSuccess))).ConfigureAwait(false);

        return resultMergingStrategy(currentSuccess, taskResults.ToList());
    }

    /// <summary>
    /// Chains an array of jobs which will be executed in parallel. This method will return once all jobs have completed.
    /// The result of the <paramref name="source"/> Task is evaluated and if it contains a failure, that failure is immediately returned.
    /// Please note, the success value is passed into each job by ref, so care must be taken around any mutation of any state on it.
    /// The naive default result merging strategy is to return the first failure if any jobs return a failure,
    /// otherwise return the original success value passed into this method.
    /// A better strategy can <i>and should</i> be provided using the overload of this method.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The resulting Task of the previous link in the chain.</param>
    /// <param name="tasks">A list of jobs to execute in parallel.</param>
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static async Task<Result<TSuccess, TFailure>> ThenWaitForAll<TSuccess, TFailure>(
        this Task<Result<TSuccess, TFailure>> source,
        params Func<TSuccess, Task<Result<TSuccess, TFailure>>>[] tasks)
    {
        static Result<TSuccess, TFailure> DefaultResultMergingStrategy(TSuccess input, List<Result<TSuccess, TFailure>> results)
        {
            return results.Any(x => x.IsFailure) ? results.First(x => x.IsFailure) : input;
        }

        return await source.ThenWaitForAll(DefaultResultMergingStrategy, tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Chains an array of jobs which will be executed in parallel. This method will return immediately once the first job has completed.
    /// The result of the <paramref name="source"/> Task is evaluated and if it contains a failure, that failure is immediately returned.
    /// The result of the first completed job will be returned. The other job's results are ignored.
    /// Please note, the success value is passed into each job by ref, so care must be taken around any mutation of any state on it.
    /// </summary>
    /// <typeparam name="TSuccess">Represents success, also likely contains any required state/results for processing in the chain.</typeparam>
    /// <typeparam name="TFailure">Represents a failure at some point in the chain.</typeparam>
    /// <param name="source">The resulting Task of the previous link in the chain.</param>
    /// <param name="tasks">A list of jobs to execute in parallel.</param>
    /// <returns>A Task of <see cref="Result{TSuccess, TFailure}"/>, which enables these extension methods to form a chain.</returns>
    public static async Task<Result<TSuccess, TFailure>> ThenWaitForFirst<TSuccess, TFailure>(
        this Task<Result<TSuccess, TFailure>> source,
        params Func<TSuccess, Task<Result<TSuccess, TFailure>>>[] tasks)
    {
        var successOrFailure = await source.ConfigureAwait(false);
        if (successOrFailure.IsFailure)
            return successOrFailure;

        if (tasks.Length == 0)
            return successOrFailure;

        var currentSuccess = successOrFailure.SuccessValue;
        return await (await Task.WhenAny(tasks.Select(task => task(currentSuccess))).ConfigureAwait(false)).ConfigureAwait(false);
    }
}
