namespace PipeEx.ConditionalExpressions;

public static class IfExpressions
{
    /// <summary>
    /// Conditionally transforms the source object based on a predicate.
    /// If the <paramref name="predicate"/> is false, the original <paramref name="source"/> object is returned.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean. Must not be null.</param>
    /// <param name="transform">The transformation function to apply if the <paramref name="predicate"/> is true. Must not be null.</param>
    /// <returns>The transformed source object if the <paramref name="predicate"/> is true, otherwise the original <paramref name="source"/> object.</returns>
    public static TSource If<TSource>(this TSource source, Func<TSource, bool> predicate, Func<TSource, TSource> transform) 
        => predicate(source) ? transform(source) : source;

    /// <summary>
    /// Conditionally transforms the source object based on a predicate, providing alternative transformations for both true and false cases.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result after transformation.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean. Must not be null.</param>
    /// <param name="transform">The transformation function to apply if the <paramref name="predicate"/> is true. Must not be null.</param>
    /// <param name="elseTransform">The transformation function to apply if the <paramref name="predicate"/> is false. Must not be null.</param>
    /// <returns>The result of applying either <paramref name="transform"/> or <paramref name="elseTransform"/> based on the predicate.</returns>
    public static TResult If<TSource, TResult>(this TSource source, Func<TSource, bool> predicate, Func<TSource, TResult> transform, Func<TSource, TResult> elseTransform) 
        => predicate(source) ? transform(source) : elseTransform(source);
}