namespace PipeEx.ConditionalExpressions;

/// <summary>
/// A set of extension methods which form a lazily evaluated, value producing if / else if / else chain.
/// The chain is started with <c>Switch</c>, extended with any number of <c>ElseIf</c> branches and must be
/// terminated with <c>Else</c>, which produces the final value. The first branch whose predicate returns true wins;
/// predicates and transformations of later branches are not evaluated.
/// </summary>
public static class SwitchExpressions
{
    /// <summary>
    /// Starts a value producing conditional chain.
    /// If the <paramref name="predicate"/> is true, the <paramref name="transform"/> is applied and later branches are skipped.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="transform">The transformation function to apply if the <paramref name="predicate"/> is true.</param>
    /// <returns>A <see cref="SwitchExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static SwitchExpression<TSource, TResult> Switch<TSource, TResult>(this TSource source, Func<TSource, bool> predicate, Func<TSource, TResult> transform)
        => predicate(source)
            ? new SwitchExpression<TSource, TResult>(source, true, transform(source))
            : new SwitchExpression<TSource, TResult>(source, false, default);

    /// <summary>
    /// Starts a value producing conditional chain with a constant branch value.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="value">The value the chain produces if the <paramref name="predicate"/> is true.</param>
    /// <returns>A <see cref="SwitchExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static SwitchExpression<TSource, TResult> Switch<TSource, TResult>(this TSource source, Func<TSource, bool> predicate, TResult value)
        => predicate(source)
            ? new SwitchExpression<TSource, TResult>(source, true, value)
            : new SwitchExpression<TSource, TResult>(source, false, default);

    /// <summary>
    /// Starts a value producing conditional chain with an asynchronous transformation.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="transform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is true.</param>
    /// <returns>A task containing a <see cref="SwitchExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static async Task<SwitchExpression<TSource, TResult>> Switch<TSource, TResult>(this TSource source, Func<TSource, bool> predicate, Func<TSource, Task<TResult>> transform)
        => predicate(source)
            ? new SwitchExpression<TSource, TResult>(source, true, await transform(source))
            : new SwitchExpression<TSource, TResult>(source, false, default);

    /// <summary>
    /// Awaits the source task and starts a value producing conditional chain on the result.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="transform">The transformation function to apply if the <paramref name="predicate"/> is true.</param>
    /// <returns>A task containing a <see cref="SwitchExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static async Task<SwitchExpression<TSource, TResult>> Switch<TSource, TResult>(this Task<TSource> source, Func<TSource, bool> predicate, Func<TSource, TResult> transform)
        => (await source).Switch(predicate, transform);

    /// <summary>
    /// Awaits the source task and starts a value producing conditional chain on the result, with an asynchronous transformation.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="transform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is true.</param>
    /// <returns>A task containing a <see cref="SwitchExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static async Task<SwitchExpression<TSource, TResult>> Switch<TSource, TResult>(this Task<TSource> source, Func<TSource, bool> predicate, Func<TSource, Task<TResult>> transform)
        => await (await source).Switch(predicate, transform);

    /// <summary>
    /// Awaits the source task and starts a value producing conditional chain on the result, with a constant branch value.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the source object.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="value">The value the chain produces if the <paramref name="predicate"/> is true.</param>
    /// <returns>A task containing a <see cref="SwitchExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static async Task<SwitchExpression<TSource, TResult>> Switch<TSource, TResult>(this Task<TSource> source, Func<TSource, bool> predicate, TResult value)
        => (await source).Switch(predicate, value);

    /// <summary>
    /// Adds a branch to the chain. The branch is only evaluated if no previous branch has matched.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The chain built so far.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="transform">The transformation function to apply if the <paramref name="predicate"/> is true.</param>
    /// <returns>A <see cref="SwitchExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static SwitchExpression<TSource, TResult> ElseIf<TSource, TResult>(this SwitchExpression<TSource, TResult> source, Func<TSource, bool> predicate, Func<TSource, TResult> transform)
        => source.IsMatched || !predicate(source.Source)
            ? source
            : new SwitchExpression<TSource, TResult>(source.Source, true, transform(source.Source));

    /// <summary>
    /// Adds a branch with a constant value to the chain. The branch is only evaluated if no previous branch has matched.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The chain built so far.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="value">The value the chain produces if the <paramref name="predicate"/> is true.</param>
    /// <returns>A <see cref="SwitchExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static SwitchExpression<TSource, TResult> ElseIf<TSource, TResult>(this SwitchExpression<TSource, TResult> source, Func<TSource, bool> predicate, TResult value)
        => source.IsMatched || !predicate(source.Source)
            ? source
            : new SwitchExpression<TSource, TResult>(source.Source, true, value);

    /// <summary>
    /// Adds a branch with an asynchronous transformation to the chain. The branch is only evaluated if no previous branch has matched.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The chain built so far.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="transform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is true.</param>
    /// <returns>A task containing a <see cref="SwitchExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static async Task<SwitchExpression<TSource, TResult>> ElseIf<TSource, TResult>(this SwitchExpression<TSource, TResult> source, Func<TSource, bool> predicate, Func<TSource, Task<TResult>> transform)
        => source.IsMatched || !predicate(source.Source)
            ? source
            : new SwitchExpression<TSource, TResult>(source.Source, true, await transform(source.Source));

    /// <summary>
    /// Awaits the chain built so far and adds a branch. The branch is only evaluated if no previous branch has matched.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the chain built so far.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="transform">The transformation function to apply if the <paramref name="predicate"/> is true.</param>
    /// <returns>A task containing a <see cref="SwitchExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static async Task<SwitchExpression<TSource, TResult>> ElseIf<TSource, TResult>(this Task<SwitchExpression<TSource, TResult>> source, Func<TSource, bool> predicate, Func<TSource, TResult> transform)
        => (await source).ElseIf(predicate, transform);

    /// <summary>
    /// Awaits the chain built so far and adds a branch with an asynchronous transformation. The branch is only evaluated if no previous branch has matched.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the chain built so far.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="transform">The asynchronous transformation function to apply if the <paramref name="predicate"/> is true.</param>
    /// <returns>A task containing a <see cref="SwitchExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static async Task<SwitchExpression<TSource, TResult>> ElseIf<TSource, TResult>(this Task<SwitchExpression<TSource, TResult>> source, Func<TSource, bool> predicate, Func<TSource, Task<TResult>> transform)
        => await (await source).ElseIf(predicate, transform);

    /// <summary>
    /// Awaits the chain built so far and adds a branch with a constant value. The branch is only evaluated if no previous branch has matched.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the chain built so far.</param>
    /// <param name="predicate">A function that evaluates the source object and returns a boolean.</param>
    /// <param name="value">The value the chain produces if the <paramref name="predicate"/> is true.</param>
    /// <returns>A task containing a <see cref="SwitchExpression{TSource, TResult}"/> to be continued with <c>ElseIf</c> or terminated with <c>Else</c>.</returns>
    public static async Task<SwitchExpression<TSource, TResult>> ElseIf<TSource, TResult>(this Task<SwitchExpression<TSource, TResult>> source, Func<TSource, bool> predicate, TResult value)
        => (await source).ElseIf(predicate, value);

    /// <summary>
    /// Terminates the chain and produces the final value.
    /// If a previous branch matched, its result is returned; otherwise <paramref name="elseTransform"/> is applied.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The chain built so far.</param>
    /// <param name="elseTransform">The transformation function to apply if no previous branch matched.</param>
    /// <returns>The result of the first matching branch, or of <paramref name="elseTransform"/> if no branch matched.</returns>
    public static TResult Else<TSource, TResult>(this SwitchExpression<TSource, TResult> source, Func<TSource, TResult> elseTransform)
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
    public static TResult Else<TSource, TResult>(this SwitchExpression<TSource, TResult> source, TResult value)
        => source.IsMatched ? source.Result! : value;

    /// <summary>
    /// Terminates the chain and produces the final value, applying an asynchronous transformation if no previous branch matched.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The chain built so far.</param>
    /// <param name="elseTransform">The asynchronous transformation function to apply if no previous branch matched.</param>
    /// <returns>A task containing the result of the first matching branch, or of <paramref name="elseTransform"/> if no branch matched.</returns>
    public static async Task<TResult> Else<TSource, TResult>(this SwitchExpression<TSource, TResult> source, Func<TSource, Task<TResult>> elseTransform)
        => source.IsMatched ? source.Result! : await elseTransform(source.Source);

    /// <summary>
    /// Awaits the chain built so far, terminates it and produces the final value.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the chain built so far.</param>
    /// <param name="elseTransform">The transformation function to apply if no previous branch matched.</param>
    /// <returns>A task containing the result of the first matching branch, or of <paramref name="elseTransform"/> if no branch matched.</returns>
    public static async Task<TResult> Else<TSource, TResult>(this Task<SwitchExpression<TSource, TResult>> source, Func<TSource, TResult> elseTransform)
        => (await source).Else(elseTransform);

    /// <summary>
    /// Awaits the chain built so far, terminates it and produces the final value, applying an asynchronous transformation if no previous branch matched.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the chain built so far.</param>
    /// <param name="elseTransform">The asynchronous transformation function to apply if no previous branch matched.</param>
    /// <returns>A task containing the result of the first matching branch, or of <paramref name="elseTransform"/> if no branch matched.</returns>
    public static async Task<TResult> Else<TSource, TResult>(this Task<SwitchExpression<TSource, TResult>> source, Func<TSource, Task<TResult>> elseTransform)
        => await (await source).Else(elseTransform);

    /// <summary>
    /// Awaits the chain built so far, terminates it and produces the final value.
    /// If a previous branch matched, its result is returned; otherwise <paramref name="value"/> is returned.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
    /// <param name="source">The task producing the chain built so far.</param>
    /// <param name="value">The value to return if no previous branch matched.</param>
    /// <returns>A task containing the result of the first matching branch, or <paramref name="value"/> if no branch matched.</returns>
    public static async Task<TResult> Else<TSource, TResult>(this Task<SwitchExpression<TSource, TResult>> source, TResult value)
        => (await source).Else(value);
}

/// <summary>
/// Represents a value producing conditional chain in progress. Created by <c>Switch</c>, continued with <c>ElseIf</c>
/// and terminated with <c>Else</c>, which unwraps the final value.
/// </summary>
/// <typeparam name="TSource">The type of the source object.</typeparam>
/// <typeparam name="TResult">The type of the result produced by the chain.</typeparam>
public sealed class SwitchExpression<TSource, TResult>
{
    internal SwitchExpression(TSource source, bool isMatched, TResult? result)
    {
        Source = source;
        IsMatched = isMatched;
        Result = result;
    }

    internal TSource Source { get; }
    internal bool IsMatched { get; }
    internal TResult? Result { get; }
}
