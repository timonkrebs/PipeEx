namespace PipeEx;

public static class Core
{
    /// <summary>
    /// Applies a transformation function to the source object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result object.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>The result of the transformation.</returns>
    public static TResult I<TSource, TResult>(this TSource source, Func<TSource, TResult> transform)
    {
        return transform(source);
    }
}