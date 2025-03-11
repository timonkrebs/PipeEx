namespace PipeEx;

public static class TupleDestructuring
{

    /// <summary>
    /// Applies a transformation function to the source object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result object.</typeparam>
    /// <param name="source">The source object tuple.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>The result of the transformation.</returns>
    public static TResult I<TSource1, TSource2, TResult>(this (TSource1, TSource2) source, Func<TSource1, TSource2, TResult> transform)
    {
        return transform(source.Item1, source.Item2);
    }

    /// <summary>
    /// Applies a transformation function to the source object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result object.</typeparam>
    /// <param name="source">The source object tuple.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>The result of the transformation.</returns>
    public static TResult I<TSource1, TSource2, TSource3, TResult>(this (TSource1, TSource2, TSource3) source, Func<TSource1, TSource2, TSource3, TResult> transform)
    {
        return transform(source.Item1, source.Item2, source.Item3);
    }

    /// <summary>
    /// Applies a transformation function to the source object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result object.</typeparam>
    /// <param name="source">The source object tuple.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>The result of the transformation.</returns>
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TResult>(this (TSource1, TSource2, TSource3, TSource4) source, Func<TSource1, TSource2, TSource3, TSource4, TResult> transform)
    {
        return transform(source.Item1, source.Item2, source.Item3, source.Item4);
    }

    /// <summary>
    /// Applies a transformation function to the source object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result object.</typeparam>
    /// <param name="source">The source object tuple.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>The result of the transformation.</returns>
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult> transform)
    {
        return transform(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5);
    }

    /// <summary>
    /// Applies a transformation function to the source object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result object.</typeparam>
    /// <param name="source">The source object tuple.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>The result of the transformation.</returns>
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult> transform)
    {
        return transform(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6);
    }

    /// <summary>
    /// Applies a transformation function to the source object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result object.</typeparam>
    /// <param name="source">The source object tuple.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>The result of the transformation.</returns>
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult> transform)
    {
        return transform(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7);
    }

    /// <summary>
    /// Applies a transformation function to the source object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result object.</typeparam>
    /// <param name="source">The source object tuple.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>The result of the transformation.</returns>
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult> transform)
    {
        return transform(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8);
    }

    /// <summary>
    /// Applies a transformation function to the source object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result object.</typeparam>
    /// <param name="source">The source object tuple.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>The result of the transformation.</returns>
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult> transform)
    {
        return transform(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9);
    }

    /// <summary>
    /// Applies a transformation function to the source object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result object.</typeparam>
    /// <param name="source">The source object tuple.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>The result of the transformation.</returns>
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult> transform)
    {
        return transform(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10);
    }

    /// <summary>
    /// Applies a transformation function to the source object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result object.</typeparam>
    /// <param name="source">The source object tuple.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>The result of the transformation.</returns>
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult> transform)
    {
        return transform(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11);
    }

    /// <summary>
    /// Applies a transformation function to the source object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result object.</typeparam>
    /// <param name="source">The source object tuple.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>The result of the transformation.</returns>
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult> transform)
    {
        return transform(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12);
    }

    /// <summary>
    /// Applies a transformation function to the source object.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result object.</typeparam>
    /// <param name="source">The source object tuple.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>The result of the transformation.</returns>
    public static TResult I<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13) source, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult> transform)
    {
        return transform(source.Item1, source.Item2, source.Item3, source.Item4, source.Item5, source.Item6, source.Item7, source.Item8, source.Item9, source.Item10, source.Item11, source.Item12, source.Item13);
    }
}
