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
}