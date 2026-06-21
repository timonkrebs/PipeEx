using System.Runtime.CompilerServices;

namespace PipeEx.StructuredConcurrency;

/// <summary>
/// Base type for structured tasks; owns (or shares) the <see cref="CancellationTokenSource"/> that
/// governs the structured-concurrency scope and provides a <see cref="Dispose"/> hook to release it.
/// </summary>
public abstract class StructuredTask
{
    internal bool MustHandleDisposing { get; set; }

    /// <summary>Gets the <see cref="System.Threading.CancellationTokenSource"/> for this structured scope.</summary>
    public CancellationTokenSource CancellationTokenSource { get; }

    /// <summary>
    /// Initialises the base with a cancellation token source, optionally taking ownership of its disposal.
    /// </summary>
    /// <param name="cancellationTokenSource">The cancellation source for this scope.</param>
    /// <param name="mustHandleDisposing"><see langword="true"/> if this instance is responsible for disposing <paramref name="cancellationTokenSource"/>.</param>
    public StructuredTask(CancellationTokenSource cancellationTokenSource, bool mustHandleDisposing = true)
    {
        MustHandleDisposing = mustHandleDisposing;
        CancellationTokenSource = cancellationTokenSource;
    }

    /// <summary>Disposes the <see cref="CancellationTokenSource"/> if this instance owns it.</summary>
    public void Dispose() { if (MustHandleDisposing) CancellationTokenSource.Dispose(); }
}

/// <summary>
/// A task-like type that pairs a <see cref="Task{T}"/> with a structured-concurrency scope.
/// Supports <c>async</c>/<c>await</c> via <see cref="AsyncStructuredTaskMethodBuilder{T}"/> and
/// implicit conversion to <see cref="Task{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
[AsyncMethodBuilder(typeof(AsyncStructuredTaskMethodBuilder<>))]
public class StructuredTask<T> : StructuredTask, IDisposable
{
    /// <summary>
    /// Creates a chained task that transfers cancellation-source ownership from <paramref name="previousTask"/>.
    /// </summary>
    public StructuredTask(Task<T> task, StructuredTask previousTask)
        : this(task, previousTask.CancellationTokenSource) { previousTask.MustHandleDisposing = false; }

    /// <summary>
    /// Creates a task linked to an external cancellation token (does not take ownership of its source).
    /// </summary>
    public StructuredTask(Task<T> task, CancellationToken cancellationToken)
        : this(task, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken), false) { }

    internal StructuredTask(Task<T> task) : this(task, new CancellationTokenSource()) { }

    internal StructuredTask(Task<T> task, CancellationTokenSource cancellationTokenSource, bool mustHandleDisposing = true)
        : base(cancellationTokenSource, mustHandleDisposing) { Task = task; }

    internal Task<T> Task { get; }

    /// <summary>Returns the awaiter for the underlying task.</summary>
    public TaskAwaiter<T> GetAwaiter() => Task.GetAwaiter();

    /// <summary>
    /// Configures how awaits on this task are performed, delegating to the underlying <see cref="System.Threading.Tasks.Task{T}"/>.
    /// </summary>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false.</param>
    public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext) => Task.ConfigureAwait(continueOnCapturedContext);

    /// <summary>Implicitly converts a <see cref="StructuredTask{T}"/> to its underlying <see cref="Task{T}"/>.</summary>
    public static implicit operator Task<T>(StructuredTask<T> structuredTask) => structuredTask.Task;
}

/// <summary>
/// Extends <see cref="StructuredTask{T}"/> to carry a single deferred task alongside the primary source task.
/// Created by <c>Let</c> and consumed by <c>Await</c>.
/// </summary>
/// <typeparam name="T">The type of the primary source value.</typeparam>
/// <typeparam name="TDeferred">The type produced by the deferred task.</typeparam>
public class StructuredDeferredTask<T, TDeferred> : StructuredTask<T>
{
    internal readonly Task<TDeferred> deferredTask1;

    internal StructuredDeferredTask(Task<T> task, Task<TDeferred> deferredTask)
        : this(task, deferredTask, new CancellationTokenSource()) { }

    internal StructuredDeferredTask(StructuredTask<T> task, Task<TDeferred> deferredTask)
        : this(task, deferredTask, task.CancellationTokenSource) { task.MustHandleDisposing = false; }

    internal StructuredDeferredTask(Task<T> task, Task<TDeferred> deferredTask1, CancellationTokenSource cancellationTokenSource)
        : base(task, cancellationTokenSource) { this.deferredTask1 = deferredTask1; }
}

/// <summary>
/// Extends <see cref="StructuredDeferredTask{T, TDeferred1}"/> to carry a second deferred task,
/// supporting two-argument <c>Let</c> chains awaited by the three-argument <c>Await</c> overload.
/// </summary>
/// <typeparam name="T">The type of the primary source value.</typeparam>
/// <typeparam name="TDeferred1">The type produced by the first deferred task.</typeparam>
/// <typeparam name="TDeferred2">The type produced by the second deferred task.</typeparam>
public class StructuredDeferredTask<T, TDeferred1, TDeferred2> : StructuredDeferredTask<T, TDeferred1>
{
    internal readonly Task<TDeferred2> deferredTask2;

    internal StructuredDeferredTask(Task<T> task, Task<TDeferred1> deferredTask1, Task<TDeferred2> deferredTask2)
        : this(task, deferredTask1, deferredTask2, new CancellationTokenSource()) { }

    internal StructuredDeferredTask(Task<T> task, Task<TDeferred1> deferredTask1, Task<TDeferred2> deferredTask2, CancellationTokenSource cancellationTokenSource)
        : base(task, deferredTask1, cancellationTokenSource) { this.deferredTask2 = deferredTask2; }

    // Extends an existing two-element deferred chain, reusing the source's CancellationTokenSource (with
    // ownership transfer) so cancellation continues to flow through the newly added stage.
    internal StructuredDeferredTask(StructuredDeferredTask<T, TDeferred1> source, Task<TDeferred2> deferredTask2)
        : this(source.Task, source.deferredTask1, deferredTask2, source.CancellationTokenSource) { source.MustHandleDisposing = false; }
}

/// <summary>
/// Async method builder that lets <c>async</c> methods return a <see cref="StructuredTask{T}"/>;
/// it forwards to <see cref="AsyncTaskMethodBuilder{T}"/> and wraps the resulting task.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
public struct AsyncStructuredTaskMethodBuilder<T>
{
    private AsyncTaskMethodBuilder<T> _builder;

    /// <summary>Creates a new builder instance.</summary>
    public static AsyncStructuredTaskMethodBuilder<T> Create() => default;

    /// <inheritdoc cref="AsyncTaskMethodBuilder{T}.Start{TStateMachine}"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
        _builder.Start(ref stateMachine);

    /// <inheritdoc cref="AsyncTaskMethodBuilder{T}.SetStateMachine"/>
    public void SetStateMachine(IAsyncStateMachine stateMachine) =>
        _builder.SetStateMachine(stateMachine);

    /// <inheritdoc cref="AsyncTaskMethodBuilder{T}.SetResult"/>
    public void SetResult(T result) =>
        _builder.SetResult(result);

    /// <inheritdoc cref="AsyncTaskMethodBuilder{T}.SetException"/>
    public void SetException(Exception exception) =>
        _builder.SetException(exception);

    /// <inheritdoc cref="AsyncTaskMethodBuilder{T}.AwaitOnCompleted{TAwaiter, TStateMachine}"/>
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        _builder.AwaitOnCompleted(ref awaiter, ref stateMachine);

    /// <inheritdoc cref="AsyncTaskMethodBuilder{T}.AwaitUnsafeOnCompleted{TAwaiter, TStateMachine}"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        _builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);

    /// <summary>Gets the <see cref="StructuredTask{T}"/> for the async method.</summary>
    public StructuredTask<T> Task => new StructuredTask<T>(_builder.Task);
}
