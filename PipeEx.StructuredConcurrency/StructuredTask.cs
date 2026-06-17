using System.Runtime.CompilerServices;

namespace PipeEx.StructuredConcurrency;

public abstract class StructuredTask
{
    internal bool MustHandleDisposing { get; set; }

    public CancellationTokenSource CancellationTokenSource { get; }

    public StructuredTask(CancellationTokenSource cancellationTokenSource, bool mustHandleDisposing = true)
    {
        MustHandleDisposing = mustHandleDisposing;
        CancellationTokenSource = cancellationTokenSource;
    }

    public void Dispose() { if (MustHandleDisposing) CancellationTokenSource.Dispose(); }
}

[AsyncMethodBuilder(typeof(AsyncStructuredTaskMethodBuilder<>))]
public class StructuredTask<T> : StructuredTask, IDisposable
{
    public StructuredTask(Task<T> task, StructuredTask previousTask)
        : this(task, previousTask.CancellationTokenSource) { previousTask.MustHandleDisposing = false; }

    public StructuredTask(Task<T> task, CancellationToken cancellationToken)
        : this(task, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken), false) { }

    internal StructuredTask(Task<T> task) : this(task, new CancellationTokenSource()) { }

    internal StructuredTask(Task<T> task, CancellationTokenSource cancellationTokenSource, bool mustHandleDisposing = true) 
        : base(cancellationTokenSource, mustHandleDisposing) { Task = task; }

    internal Task<T> Task { get; }

    public TaskAwaiter<T> GetAwaiter() => Task.GetAwaiter();

    /// <summary>
    /// Configures how awaits on this task are performed, delegating to the underlying <see cref="System.Threading.Tasks.Task{T}"/>.
    /// </summary>
    /// <param name="continueOnCapturedContext">true to attempt to marshal the continuation back to the original context captured; otherwise, false.</param>
    public ConfiguredTaskAwaitable<T> ConfigureAwait(bool continueOnCapturedContext) => Task.ConfigureAwait(continueOnCapturedContext);

    public static implicit operator Task<T>(StructuredTask<T> structuredTask) => structuredTask.Task;
}

public class StructuredDeferredTask<T, TDeferred> : StructuredTask<T>
{
    internal Task<TDeferred> deferredTask1;

    internal StructuredDeferredTask(Task<T> task, Task<TDeferred> deferredTask)
        : this(task, deferredTask, new CancellationTokenSource()) { }

    internal StructuredDeferredTask(StructuredTask<T> task, Task<TDeferred> deferredTask)
        : this(task, deferredTask, task.CancellationTokenSource) { task.MustHandleDisposing = false; }

    internal StructuredDeferredTask(Task<T> task, Task<TDeferred> deferredTask1, CancellationTokenSource cancellationTokenSource)
        : base(task, cancellationTokenSource) { this.deferredTask1 = deferredTask1; }
}

public class StructuredDeferredTask<T, TDeferred1, TDeferred2> : StructuredDeferredTask<T, TDeferred1>
{
    internal Task<TDeferred2> deferredTask2;

    internal StructuredDeferredTask(Task<T> task, Task<TDeferred1> deferredTask1, Task<TDeferred2> deferredTask2)
        : this(task, deferredTask1, deferredTask2, new CancellationTokenSource()) { }

    internal StructuredDeferredTask(Task<T> task, Task<TDeferred1> deferredTask1, Task<TDeferred2> deferredTask2, CancellationTokenSource cancellationTokenSource)
        : base(task, deferredTask1, cancellationTokenSource) { this.deferredTask2 = deferredTask2; }
}

/// <summary>
/// Async method builder that lets <c>async</c> methods return a <see cref="StructuredTask{T}"/>;
/// it forwards to <see cref="AsyncTaskMethodBuilder{T}"/> and wraps the resulting task.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
public struct AsyncStructuredTaskMethodBuilder<T>
{
    private AsyncTaskMethodBuilder<T> _builder;

    public static AsyncStructuredTaskMethodBuilder<T> Create() => default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
        _builder.Start(ref stateMachine);

    public void SetStateMachine(IAsyncStateMachine stateMachine) =>
        _builder.SetStateMachine(stateMachine);

    public void SetResult(T result) =>
        _builder.SetResult(result);

    public void SetException(Exception exception) =>
        _builder.SetException(exception);

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        _builder.AwaitOnCompleted(ref awaiter, ref stateMachine);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        _builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);

    public StructuredTask<T> Task => new StructuredTask<T>(_builder.Task);
}
