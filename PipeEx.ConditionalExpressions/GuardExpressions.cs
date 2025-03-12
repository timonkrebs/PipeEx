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
        if (!predicate(source)) return new ConditionalExecutionResult<TSource>(source, true); // IsSkipped = true when predicate is false

        action(source);
        return new ConditionalExecutionResult<TSource>(source); // IsSkipped = false (default) when predicate is true and action is executed
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
        if (sourceResult.Skip) return sourceResult; // Short-circuit: if already skipped, propagate the skipped status
        if (!predicate(sourceResult.Value)) return new ConditionalExecutionResult<TSource>(sourceResult.Value, true); // Mark as skipped if current predicate is false

        action(sourceResult.Value);
        return sourceResult; // Condition met, action executed, propagate the same ConditionalExecutionResult
    }

    /// <summary>
    /// Executes an alternative action if the previous condition in the chain was skipped.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="sourceResult">The ConditionalExecutionResult from the previous step.</param>
    /// <param name="action">The action to execute if the previous condition was skipped.</param>
    /// <returns>Unwrapped ConditionalExecutionResult.</returns>
    public static TSource Else<TSource>(this ConditionalExecutionResult<TSource> sourceResult, Action<TSource> action)
    {
        if (!sourceResult.Skip) return sourceResult.Value; // If not skipped, do nothing and propagate the same result

        action(sourceResult.Value); // Execute the else action
        return sourceResult.Value; // After Else action, reset IsSkipped to false, as we've handled the 'else' case.
    }
}

public class ConditionalExecutionResult<TSource>
{
    public ConditionalExecutionResult(TSource value, bool skip = false)
    {
        Value = value;
        Skip = skip;
    }

    public TSource Value { get; }
    public bool Skip { get; }

    public static implicit operator TSource(ConditionalExecutionResult<TSource> conditionalExecutionResult)
    {
        return conditionalExecutionResult.Value;
    }
}