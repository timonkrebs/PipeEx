namespace PipeEx.ConditionalExpressions;

public static class GuardExpressions
{
    /// <summary>
    /// Conditionally executes an action on the source object based on a predicate, returning a ConditionalExecutionResult for chaining.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="action">The action to execute if the predicate is true.</param>
    /// <returns>A ConditionalExecutionResult wrapping the source object.</returns>
    public static ConditionalExecutionResult<TSource> Guard<TSource>(this TSource source, Func<TSource, bool> predicate, Action<TSource> action)
    {
        if (!predicate(source)) return new ConditionalExecutionResult<TSource>(source, true);

        action(source);
        return new ConditionalExecutionResult<TSource>(source);
    }

    /// <summary>
    /// Conditionally executes an action on the source object if the previous condition in the chain was not skipped.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="sourceResult">The ConditionalExecutionResult from the previous step.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="action">The action to execute if the predicate is true and the previous condition was not skipped.</param>
    /// <returns>The updated ConditionalExecutionResult.</returns>
    public static ConditionalExecutionResult<TSource> Guard<TSource>(this ConditionalExecutionResult<TSource> sourceResult, Func<TSource, bool> predicate, Action<TSource> action)
    {
        if (sourceResult.Skip) return sourceResult;
        if (!predicate(sourceResult.Value)) return new ConditionalExecutionResult<TSource>(sourceResult.Value, true);

        action(sourceResult.Value);
        return sourceResult;
    }

    /// <summary>
    /// Conditionally executes an asynchronous action on the source object based on a predicate, returning a ConditionalExecutionResult for chaining.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="action">The asynchronous action to execute if the predicate is true.</param>
    /// <returns>A task containing a ConditionalExecutionResult wrapping the source object.</returns>
    public static async Task<ConditionalExecutionResult<TSource>> Guard<TSource>(this TSource source, Func<TSource, bool> predicate, Func<TSource, Task> action)
    {
        if (!predicate(source)) return new ConditionalExecutionResult<TSource>(source, true);

        await action(source).ConfigureAwait(false);
        return new ConditionalExecutionResult<TSource>(source);
    }

    /// <summary>
    /// Awaits the source task and conditionally executes an action on the result based on a predicate, returning a ConditionalExecutionResult for chaining.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="action">The action to execute if the predicate is true.</param>
    /// <returns>A task containing a ConditionalExecutionResult wrapping the awaited source object.</returns>
    public static async Task<ConditionalExecutionResult<TSource>> Guard<TSource>(this Task<TSource> source, Func<TSource, bool> predicate, Action<TSource> action)
        => (await source.ConfigureAwait(false)).Guard(predicate, action);

    /// <summary>
    /// Awaits the source task and conditionally executes an asynchronous action on the result based on a predicate, returning a ConditionalExecutionResult for chaining.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="action">The asynchronous action to execute if the predicate is true.</param>
    /// <returns>A task containing a ConditionalExecutionResult wrapping the awaited source object.</returns>
    public static async Task<ConditionalExecutionResult<TSource>> Guard<TSource>(this Task<TSource> source, Func<TSource, bool> predicate, Func<TSource, Task> action)
        => await (await source.ConfigureAwait(false)).Guard(predicate, action).ConfigureAwait(false);

    /// <summary>
    /// Conditionally executes an asynchronous action on the source object if the previous condition in the chain was not skipped.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="sourceResult">The ConditionalExecutionResult from the previous step.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="action">The asynchronous action to execute if the predicate is true and the previous condition was not skipped.</param>
    /// <returns>A task containing the updated ConditionalExecutionResult.</returns>
    public static async Task<ConditionalExecutionResult<TSource>> Guard<TSource>(this ConditionalExecutionResult<TSource> sourceResult, Func<TSource, bool> predicate, Func<TSource, Task> action)
    {
        if (sourceResult.Skip) return sourceResult;
        if (!predicate(sourceResult.Value)) return new ConditionalExecutionResult<TSource>(sourceResult.Value, true);

        await action(sourceResult.Value).ConfigureAwait(false);
        return sourceResult;
    }

    /// <summary>
    /// Awaits the previous step and conditionally executes an action on the result if the previous condition in the chain was not skipped.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="sourceResult">The task producing the ConditionalExecutionResult from the previous step.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="action">The action to execute if the predicate is true and the previous condition was not skipped.</param>
    /// <returns>A task containing the updated ConditionalExecutionResult.</returns>
    public static async Task<ConditionalExecutionResult<TSource>> Guard<TSource>(this Task<ConditionalExecutionResult<TSource>> sourceResult, Func<TSource, bool> predicate, Action<TSource> action)
        => (await sourceResult.ConfigureAwait(false)).Guard(predicate, action);

    /// <summary>
    /// Awaits the previous step and conditionally executes an asynchronous action on the result if the previous condition in the chain was not skipped.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="sourceResult">The task producing the ConditionalExecutionResult from the previous step.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="action">The asynchronous action to execute if the predicate is true and the previous condition was not skipped.</param>
    /// <returns>A task containing the updated ConditionalExecutionResult.</returns>
    public static async Task<ConditionalExecutionResult<TSource>> Guard<TSource>(this Task<ConditionalExecutionResult<TSource>> sourceResult, Func<TSource, bool> predicate, Func<TSource, Task> action)
        => await (await sourceResult.ConfigureAwait(false)).Guard(predicate, action).ConfigureAwait(false);

    /// <summary>
    /// Executes an alternative action if the previous condition in the chain was skipped.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="sourceResult">The ConditionalExecutionResult from the previous step.</param>
    /// <param name="action">The action to execute if the previous condition was skipped.</param>
    /// <returns>Unwrapped ConditionalExecutionResult.</returns>
    public static TSource Else<TSource>(this ConditionalExecutionResult<TSource> sourceResult, Action<TSource> action)
    {
        if (!sourceResult.Skip) return sourceResult.Value;

        action(sourceResult.Value);
        return sourceResult.Value;
    }

    /// <summary>
    /// Executes an alternative asynchronous action if the previous condition in the chain was skipped.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="sourceResult">The ConditionalExecutionResult from the previous step.</param>
    /// <param name="action">The asynchronous action to execute if the previous condition was skipped.</param>
    /// <returns>A task containing the unwrapped source object.</returns>
    public static async Task<TSource> Else<TSource>(this ConditionalExecutionResult<TSource> sourceResult, Func<TSource, Task> action)
    {
        if (!sourceResult.Skip) return sourceResult.Value;

        await action(sourceResult.Value).ConfigureAwait(false);
        return sourceResult.Value;
    }

    /// <summary>
    /// Awaits the previous step and executes an alternative action if the previous condition in the chain was skipped.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="sourceResult">The task producing the ConditionalExecutionResult from the previous step.</param>
    /// <param name="action">The action to execute if the previous condition was skipped.</param>
    /// <returns>A task containing the unwrapped source object.</returns>
    public static async Task<TSource> Else<TSource>(this Task<ConditionalExecutionResult<TSource>> sourceResult, Action<TSource> action)
        => (await sourceResult.ConfigureAwait(false)).Else(action);

    /// <summary>
    /// Awaits the previous step and executes an alternative asynchronous action if the previous condition in the chain was skipped.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="sourceResult">The task producing the ConditionalExecutionResult from the previous step.</param>
    /// <param name="action">The asynchronous action to execute if the previous condition was skipped.</param>
    /// <returns>A task containing the unwrapped source object.</returns>
    public static async Task<TSource> Else<TSource>(this Task<ConditionalExecutionResult<TSource>> sourceResult, Func<TSource, Task> action)
        => await (await sourceResult.ConfigureAwait(false)).Else(action).ConfigureAwait(false);
}

/// <summary>
/// Carries the result of a conditional <c>Guard</c> step: the original value and a flag that
/// indicates whether the guarded action was skipped (predicate was false).
/// </summary>
/// <typeparam name="TSource">The type of the wrapped value.</typeparam>
public readonly struct ConditionalExecutionResult<TSource>
{
    /// <summary>
    /// Initialises a new result, optionally marking it as skipped.
    /// </summary>
    /// <param name="value">The source value being carried through the chain.</param>
    /// <param name="skip"><see langword="true"/> if the guarded action was not executed because the predicate returned <see langword="false"/>.</param>
    public ConditionalExecutionResult(TSource value, bool skip = false)
    {
        Value = value;
        Skip = skip;
    }

    /// <summary>Gets the value being carried through the chain.</summary>
    public TSource Value { get; }

    /// <summary>Gets a value indicating whether the guarded action was skipped.</summary>
    public bool Skip { get; }
}
