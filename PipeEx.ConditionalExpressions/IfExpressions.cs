namespace PipeEx.ConditionalExpressions;

/// <summary>
/// A set of extension methods which express conditional logic as fluent expressions.
/// The two branch <c>If</c> overloads produce a value directly. The single branch <c>If</c> overloads start a
/// lazily evaluated if / else if / else chain which is extended with any number of <c>ElseIf</c> branches and must be
/// terminated with <c>Else</c>, which produces the final value. The first branch whose predicate returns true wins;
/// predicates and transformations of later branches are not evaluated.
/// </summary>
public static class IfExpressions
{
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
        => predicate(source) ? await transform(source).ConfigureAwait(false) : await elseTransform(source).ConfigureAwait(false);

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
        => predicate(source) ? await transform(source).ConfigureAwait(false) : elseTransform(source);

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
        => predicate(source) ? transform(source) : await elseTransform(source).ConfigureAwait(false);

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
        => (await source.ConfigureAwait(false)).If(predicate, transform, elseTransform);

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
        => await (await source.ConfigureAwait(false)).If(predicate, transform, elseTransform).ConfigureAwait(false);

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
        => await (await source.ConfigureAwait(false)).If(predicate, transform, elseTransform).ConfigureAwait(false);

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
        => await (await source.ConfigureAwait(false)).If(predicate, transform, elseTransform).ConfigureAwait(false);

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
        => (await source.ConfigureAwait(false)).If(predicate, thenValue, elseValue);

    /// <summary>
    /// Starts a value producing if / else if / else chain.
    /// If the <paramref name="predicate"/> is true, the <paramref name="transform"/> is applied and later branches are skipped.
    /// The chain must be terminated with <c>Else</c>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="transform">The transformation function to apply if the <paramref name="predicate"/> is true.</param>
    /// <returns>An <see cref="IfExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static IfExpression<TSource, TResult> If<TSource, TResult>(this TSource source, Func<TSource, bool> predicate, Func<TSource, TResult> transform)
        => predicate(source)
            ? new IfExpression<TSource, TResult>(source, true, transform(source))
            : new IfExpression<TSource, TResult>(source, false, default);

    /// <summary>
    /// Starts a value producing if / else if / else chain with a constant branch value.
    /// The chain must be terminated with <c>Else</c>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="value">The value the chain produces if the <paramref name="predicate"/> is true.</param>
    /// <returns>An <see cref="IfExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static IfExpression<TSource, TResult> If<TSource, TResult>(this TSource source, Func<TSource, bool> predicate, TResult value)
        => predicate(source)
            ? new IfExpression<TSource, TResult>(source, true, value)
            : new IfExpression<TSource, TResult>(source, false, default);

    /// <summary>
    /// Starts a value producing if / else if / else chain with an asynchronous transformation.
    /// The chain must be terminated with <c>Else</c>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="transform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is true.</param>
    /// <returns>A task containing an <see cref="IfExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static async Task<IfExpression<TSource, TResult>> If<TSource, TResult>(this TSource source, Func<TSource, bool> predicate, Func<TSource, Task<TResult>> transform)
        => predicate(source)
            ? new IfExpression<TSource, TResult>(source, true, await transform(source).ConfigureAwait(false))
            : new IfExpression<TSource, TResult>(source, false, default);

    /// <summary>
    /// Awaits the source task and starts a value producing if / else if / else chain on the result.
    /// The chain must be terminated with <c>Else</c>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="transform">The transformation function to apply if the <paramref name="predicate"/> is true.</param>
    /// <returns>A task containing an <see cref="IfExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static async Task<IfExpression<TSource, TResult>> If<TSource, TResult>(this Task<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> transform)
        => (await source.ConfigureAwait(false)).If(predicate, transform);

    /// <summary>
    /// Awaits the source task and starts a value producing if / else if / else chain on the result, with an asynchronous transformation.
    /// The chain must be terminated with <c>Else</c>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="transform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is true.</param>
    /// <returns>A task containing an <see cref="IfExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static async Task<IfExpression<TSource, TResult>> If<TSource, TResult>(this Task<TSource> source, Func<TSource, bool> predicate, Func<TSource, Task<TResult>> transform)
        => await (await source.ConfigureAwait(false)).If(predicate, transform).ConfigureAwait(false);

    /// <summary>
    /// Awaits the source task and starts a value producing if / else if / else chain on the result, with a constant branch value.
    /// The chain must be terminated with <c>Else</c>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="value">The value the chain produces if the <paramref name="predicate"/> is true.</param>
    /// <returns>A task containing an <see cref="IfExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static async Task<IfExpression<TSource, TResult>> If<TSource, TResult>(this Task<TSource> source, Func<TSource, bool> predicate, TResult value)
        => (await source.ConfigureAwait(false)).If(predicate, value);

    /// <summary>
    /// Adds a branch to the chain. The branch is only evaluated if no previous branch has matched.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The chain built so far.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="transform">The transformation function to apply if the <paramref name="predicate"/> is true.</param>
    /// <returns>An <see cref="IfExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static IfExpression<TSource, TResult> ElseIf<TSource, TResult>(this IfExpression<TSource, TResult> source, Func<TSource, bool> predicate, Func<TSource, TResult> transform)
        => source.IsMatched || !predicate(source.Source)
            ? source
            : new IfExpression<TSource, TResult>(source.Source, true, transform(source.Source));

    /// <summary>
    /// Adds a branch with a constant value to the chain. The branch is only evaluated if no previous branch has matched.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The chain built so far.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="value">The value the chain produces if the <paramref name="predicate"/> is true.</param>
    /// <returns>An <see cref="IfExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static IfExpression<TSource, TResult> ElseIf<TSource, TResult>(this IfExpression<TSource, TResult> source, Func<TSource, bool> predicate, TResult value)
        => source.IsMatched || !predicate(source.Source)
            ? source
            : new IfExpression<TSource, TResult>(source.Source, true, value);

    /// <summary>
    /// Adds a branch with an asynchronous transformation to the chain. The branch is only evaluated if no previous branch has matched.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The chain built so far.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="transform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is true.</param>
    /// <returns>A task containing an <see cref="IfExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static async Task<IfExpression<TSource, TResult>> ElseIf<TSource, TResult>(this IfExpression<TSource, TResult> source, Func<TSource, bool> predicate, Func<TSource, Task<TResult>> transform)
        => source.IsMatched || !predicate(source.Source)
            ? source
            : new IfExpression<TSource, TResult>(source.Source, true, await transform(source.Source).ConfigureAwait(false));

    /// <summary>
    /// Awaits the chain built so far and adds a branch. The branch is only evaluated if no previous branch has matched.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the chain built so far.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="transform">The transformation function to apply if the <paramref name="predicate"/> is true.</param>
    /// <returns>A task containing an <see cref="IfExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static async Task<IfExpression<TSource, TResult>> ElseIf<TSource, TResult>(this Task<IfExpression<TSource, TResult>> source, Func<TSource, bool> predicate, Func<TSource, TResult> transform)
        => (await source.ConfigureAwait(false)).ElseIf(predicate, transform);

    /// <summary>
    /// Awaits the chain built so far and adds a branch with an asynchronous transformation. The branch is only evaluated if no previous branch has matched.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the chain built so far.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="transform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is true.</param>
    /// <returns>A task containing an <see cref="IfExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static async Task<IfExpression<TSource, TResult>> ElseIf<TSource, TResult>(this Task<IfExpression<TSource, TResult>> source, Func<TSource, bool> predicate, Func<TSource, Task<TResult>> transform)
        => await (await source.ConfigureAwait(false)).ElseIf(predicate, transform).ConfigureAwait(false);

    /// <summary>
    /// Awaits the chain built so far and adds a branch with a constant value. The branch is only evaluated if no previous branch has matched.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the chain built so far.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="value">The value the chain produces if the <paramref name="predicate"/> is true.</param>
    /// <returns>A task containing an <see cref="IfExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static async Task<IfExpression<TSource, TResult>> ElseIf<TSource, TResult>(this Task<IfExpression<TSource, TResult>> source, Func<TSource, bool> predicate, TResult value)
        => (await source.ConfigureAwait(false)).ElseIf(predicate, value);

    /// <summary>
    /// Terminates the chain and produces the final value.
    /// If a previous branch matched, its result is returned; otherwise <paramref name="elseTransform"/> is applied.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The chain built so far.</param>
    /// <param name="elseTransform">The transformation function to apply if no previous branch matched.</param>
    /// <returns>The result of the first matching branch, or of <paramref name="elseTransform"/> if no branch matched.</returns>
    public static TResult Else<TSource, TResult>(this IfExpression<TSource, TResult> source, Func<TSource, TResult> elseTransform)
        => source.IsMatched ? source.Result! : elseTransform(source.Source);

    /// <summary>
    /// Terminates the chain and produces the final value.
    /// If a previous branch matched, its result is returned; otherwise <paramref name="value"/> is returned.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The chain built so far.</param>
    /// <param name="value">The value to return if no previous branch matched.</param>
    /// <returns>The result of the first matching branch, or <paramref name="value"/> if no branch matched.</returns>
    public static TResult Else<TSource, TResult>(this IfExpression<TSource, TResult> source, TResult value)
        => source.IsMatched ? source.Result! : value;

    /// <summary>
    /// Terminates the chain and produces the final value, applying an asynchronous transformation if no previous branch matched.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The chain built so far.</param>
    /// <param name="elseTransform">The asynchronous transformation function to apply if no previous branch matched.</param>
    /// <returns>A task containing the result of the first matching branch, or of <paramref name="elseTransform"/> if no branch matched.</returns>
    public static async Task<TResult> Else<TSource, TResult>(this IfExpression<TSource, TResult> source, Func<TSource, Task<TResult>> elseTransform)
        => source.IsMatched ? source.Result! : await elseTransform(source.Source).ConfigureAwait(false);

    /// <summary>
    /// Awaits the chain built so far, terminates it and produces the final value.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the chain built so far.</param>
    /// <param name="elseTransform">The transformation function to apply if no previous branch matched.</param>
    /// <returns>A task containing the result of the first matching branch, or of <paramref name="elseTransform"/> if no branch matched.</returns>
    public static async Task<TResult> Else<TSource, TResult>(this Task<IfExpression<TSource, TResult>> source, Func<TSource, TResult> elseTransform)
        => (await source.ConfigureAwait(false)).Else(elseTransform);

    /// <summary>
    /// Awaits the chain built so far, terminates it and produces the final value, applying an asynchronous transformation if no previous branch matched.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the chain built so far.</param>
    /// <param name="elseTransform">The asynchronous transformation function to apply if no previous branch matched.</param>
    /// <returns>A task containing the result of the first matching branch, or of <paramref name="elseTransform"/> if no branch matched.</returns>
    public static async Task<TResult> Else<TSource, TResult>(this Task<IfExpression<TSource, TResult>> source, Func<TSource, Task<TResult>> elseTransform)
        => await (await source.ConfigureAwait(false)).Else(elseTransform).ConfigureAwait(false);

    /// <summary>
    /// Awaits the chain built so far, terminates it and produces the final value.
    /// If a previous branch matched, its result is returned; otherwise <paramref name="value"/> is returned.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the chain built so far.</param>
    /// <param name="value">The value to return if no previous branch matched.</param>
    /// <returns>A task containing the result of the first matching branch, or <paramref name="value"/> if no branch matched.</returns>
    public static async Task<TResult> Else<TSource, TResult>(this Task<IfExpression<TSource, TResult>> source, TResult value)
        => (await source.ConfigureAwait(false)).Else(value);
}

/// <summary>
/// Represents a value producing if / else if / else chain in progress. Created by <c>If</c>, continued with <c>ElseIf</c>
/// and terminated with <c>Else</c>, which unwraps the final value.
/// </summary>
/// <typeparam name="TSource">The type of the source object.</typeparam>
/// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
public readonly struct IfExpression<TSource, TResult>
{
    internal IfExpression(TSource source, bool isMatched, TResult? result)
    {
        Source = source;
        IsMatched = isMatched;
        Result = result;
    }

    internal TSource Source { get; }
    internal bool IsMatched { get; }
    internal TResult? Result { get; }
}
