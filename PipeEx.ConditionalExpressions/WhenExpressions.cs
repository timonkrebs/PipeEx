namespace PipeEx.ConditionalExpressions;

public static class WhenExpressions
{
    /// <summary>
    /// Conditionally executes an action on the source object based on a predicate, returning the source object for chaining.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="action">The action to execute if the predicate is true.</param>
    /// <returns>the source object.</returns>
    public static TSource When<TSource>(this TSource source, Func<TSource, bool> predicate, Action<TSource> action)
    {
        if (predicate(source)) action(source);

        return source;
    }

    /// <summary>
    /// Conditionally executes an asynchronous action on the source object based on a predicate, returning the source object for chaining.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="action">The asynchronous action to execute if the predicate is true.</param>
    /// <returns>A task containing the source object.</returns>
    public static async Task<TSource> When<TSource>(this TSource source, Func<TSource, bool> predicate, Func<TSource, Task> action)
    {
        if (predicate(source)) await action(source).ConfigureAwait(false);

        return source;
    }

    /// <summary>
    /// Awaits the source task and conditionally executes an action on the result based on a predicate, returning the awaited value for chaining.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="action">The action to execute if the predicate is true.</param>
    /// <returns>A task containing the awaited source object.</returns>
    public static async Task<TSource> When<TSource>(this Task<TSource> source, Func<TSource, bool> predicate, Action<TSource> action)
        => (await source.ConfigureAwait(false)).When(predicate, action);

    /// <summary>
    /// Awaits the source task and conditionally executes an asynchronous action on the result based on a predicate, returning the awaited value for chaining.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="action">The asynchronous action to execute if the predicate is true.</param>
    /// <returns>A task containing the awaited source object.</returns>
    public static async Task<TSource> When<TSource>(this Task<TSource> source, Func<TSource, bool> predicate, Func<TSource, Task> action)
        => await (await source.ConfigureAwait(false)).When(predicate, action).ConfigureAwait(false);
}
