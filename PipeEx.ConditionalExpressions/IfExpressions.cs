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

    /// <summary>
    /// Selects one of two values based on a predicate.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean. Must not be null.</param>
    /// <param name="thenValue">The value to return if the <paramref name="predicate"/> is true.</param>
    /// <param name="elseValue">The value to return if the <paramref name="predicate"/> is false.</param>
    /// <returns>Either <paramref name="thenValue"/> or <paramref name="elseValue"/> based on the predicate.</returns>
    public static TResult If<TSource, TResult>(this TSource source, Func<TSource, bool> predicate, TResult thenValue, TResult elseValue)
        => predicate(source) ? thenValue : elseValue;

    /// <summary>
    /// Conditionally transforms the source object based on a predicate, using an asynchronous transformation.
    /// If the <paramref name="predicate"/> is false, the original <paramref name="source"/> object is returned.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean. Must not be null.</param>
    /// <param name="transform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is true. Must not be null.</param>
    /// <returns>A task containing the transformed source object if the <paramref name="predicate"/> is true, otherwise the original <paramref name="source"/> object.</returns>
    public static async Task<TSource> If<TSource>(this TSource source, Func<TSource, bool> predicate, Func<TSource, Task<TSource>> transform)
        => predicate(source) ? await transform(source) : source;

    /// <summary>
    /// Conditionally transforms the source object based on a predicate, providing asynchronous transformations for both true and false cases.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result after transformation.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean. Must not be null.</param>
    /// <param name="transform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is true. Must not be null.</param>
    /// <param name="elseTransform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is false. Must not be null.</param>
    /// <returns>A task containing the result of applying either <paramref name="transform"/> or <paramref name="elseTransform"/> based on the predicate.</returns>
    public static async Task<TResult> If<TSource, TResult>(this TSource source, Func<TSource, bool> predicate, Func<TSource, Task<TResult>> transform, Func<TSource, Task<TResult>> elseTransform)
        => predicate(source) ? await transform(source) : await elseTransform(source);

    /// <summary>
    /// Conditionally transforms the source object based on a predicate, providing an asynchronous transformation for the true case
    /// and a synchronous transformation for the false case.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result after transformation.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean. Must not be null.</param>
    /// <param name="transform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is true. Must not be null.</param>
    /// <param name="elseTransform">The transformation function to apply if the <paramref name="predicate"/> is false. Must not be null.</param>
    /// <returns>A task containing the result of applying either <paramref name="transform"/> or <paramref name="elseTransform"/> based on the predicate.</returns>
    public static async Task<TResult> If<TSource, TResult>(this TSource source, Func<TSource, bool> predicate, Func<TSource, Task<TResult>> transform, Func<TSource, TResult> elseTransform)
        => predicate(source) ? await transform(source) : elseTransform(source);

    /// <summary>
    /// Conditionally transforms the source object based on a predicate, providing a synchronous transformation for the true case
    /// and an asynchronous transformation for the false case.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result after transformation.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean. Must not be null.</param>
    /// <param name="transform">The transformation function to apply if the <paramref name="predicate"/> is true. Must not be null.</param>
    /// <param name="elseTransform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is false. Must not be null.</param>
    /// <returns>A task containing the result of applying either <paramref name="transform"/> or <paramref name="elseTransform"/> based on the predicate.</returns>
    public static async Task<TResult> If<TSource, TResult>(this TSource source, Func<TSource, bool> predicate, Func<TSource, TResult> transform, Func<TSource, Task<TResult>> elseTransform)
        => predicate(source) ? transform(source) : await elseTransform(source);

    /// <summary>
    /// Awaits the source task and conditionally transforms the result based on a predicate.
    /// If the <paramref name="predicate"/> is false, the original awaited value is returned.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean. Must not be null.</param>
    /// <param name="transform">The transformation function to apply if the <paramref name="predicate"/> is true. Must not be null.</param>
    /// <returns>A task containing the transformed value if the <paramref name="predicate"/> is true, otherwise the original awaited value.</returns>
    public static async Task<TSource> If<TSource>(this Task<TSource> source, Func<TSource, bool> predicate, Func<TSource, TSource> transform)
        => (await source).If(predicate, transform);

    /// <summary>
    /// Awaits the source task and conditionally transforms the result based on a predicate, using an asynchronous transformation.
    /// If the <paramref name="predicate"/> is false, the original awaited value is returned.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean. Must not be null.</param>
    /// <param name="transform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is true. Must not be null.</param>
    /// <returns>A task containing the transformed value if the <paramref name="predicate"/> is true, otherwise the original awaited value.</returns>
    public static async Task<TSource> If<TSource>(this Task<TSource> source, Func<TSource, bool> predicate, Func<TSource, Task<TSource>> transform)
        => await (await source).If(predicate, transform);

    /// <summary>
    /// Awaits the source task and conditionally transforms the result based on a predicate, providing alternative transformations for both true and false cases.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result after transformation.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean. Must not be null.</param>
    /// <param name="transform">The transformation function to apply if the <paramref name="predicate"/> is true. Must not be null.</param>
    /// <param name="elseTransform">The transformation function to apply if the <paramref name="predicate"/> is false. Must not be null.</param>
    /// <returns>A task containing the result of applying either <paramref name="transform"/> or <paramref name="elseTransform"/> based on the predicate.</returns>
    public static async Task<TResult> If<TSource, TResult>(this Task<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> transform, Func<TSource, TResult> elseTransform)
        => (await source).If(predicate, transform, elseTransform);

    /// <summary>
    /// Awaits the source task and conditionally transforms the result based on a predicate, providing asynchronous transformations for both true and false cases.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result after transformation.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean. Must not be null.</param>
    /// <param name="transform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is true. Must not be null.</param>
    /// <param name="elseTransform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is false. Must not be null.</param>
    /// <returns>A task containing the result of applying either <paramref name="transform"/> or <paramref name="elseTransform"/> based on the predicate.</returns>
    public static async Task<TResult> If<TSource, TResult>(this Task<TSource> source, Func<TSource, bool> predicate, Func<TSource, Task<TResult>> transform, Func<TSource, Task<TResult>> elseTransform)
        => await (await source).If(predicate, transform, elseTransform);

    /// <summary>
    /// Awaits the source task and conditionally transforms the result based on a predicate, providing an asynchronous transformation for the true case
    /// and a synchronous transformation for the false case.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result after transformation.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean. Must not be null.</param>
    /// <param name="transform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is true. Must not be null.</param>
    /// <param name="elseTransform">The transformation function to apply if the <paramref name="predicate"/> is false. Must not be null.</param>
    /// <returns>A task containing the result of applying either <paramref name="transform"/> or <paramref name="elseTransform"/> based on the predicate.</returns>
    public static async Task<TResult> If<TSource, TResult>(this Task<TSource> source, Func<TSource, bool> predicate, Func<TSource, Task<TResult>> transform, Func<TSource, TResult> elseTransform)
        => await (await source).If(predicate, transform, elseTransform);

    /// <summary>
    /// Awaits the source task and conditionally transforms the result based on a predicate, providing a synchronous transformation for the true case
    /// and an asynchronous transformation for the false case.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result after transformation.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean. Must not be null.</param>
    /// <param name="transform">The transformation function to apply if the <paramref name="predicate"/> is true. Must not be null.</param>
    /// <param name="elseTransform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is false. Must not be null.</param>
    /// <returns>A task containing the result of applying either <paramref name="transform"/> or <paramref name="elseTransform"/> based on the predicate.</returns>
    public static async Task<TResult> If<TSource, TResult>(this Task<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> transform, Func<TSource, Task<TResult>> elseTransform)
        => await (await source).If(predicate, transform, elseTransform);

    /// <summary>
    /// Awaits the source task and selects one of two values based on a predicate.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean. Must not be null.</param>
    /// <param name="thenValue">The value to return if the <paramref name="predicate"/> is true.</param>
    /// <param name="elseValue">The value to return if the <paramref name="predicate"/> is false.</param>
    /// <returns>A task containing either <paramref name="thenValue"/> or <paramref name="elseValue"/> based on the predicate.</returns>
    public static async Task<TResult> If<TSource, TResult>(this Task<TSource> source, Func<TSource, bool> predicate, TResult thenValue, TResult elseValue)
        => (await source).If(predicate, thenValue, elseValue);
}
