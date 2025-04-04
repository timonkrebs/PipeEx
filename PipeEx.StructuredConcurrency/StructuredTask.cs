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

[AsyncMethodBuilder(typeof(PoolingAsyncStructuredTaskMethodBuilder<>))]
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

    public static implicit operator Task<T>(StructuredTask<T> structuredTask) => structuredTask.Task;
}

public class StructuredDeferredTask<T, TDeferd> : StructuredTask<T>
{
    internal Task<TDeferd> deferredTask1;

    internal StructuredDeferredTask(Task<T> task, Task<TDeferd> deferredTask)
        : this(task, deferredTask, new CancellationTokenSource()) { }

    internal StructuredDeferredTask(StructuredTask<T> task, Task<TDeferd> deferredTask)
        : this(task, deferredTask, task.CancellationTokenSource) { task.MustHandleDisposing = false; }

    internal StructuredDeferredTask(Task<T> task, Task<TDeferd> deferredTask1, CancellationTokenSource cancellationTokenSource)
        : base(task, cancellationTokenSource) { this.deferredTask1 = deferredTask1; }
}

public class StructuredDeferredTask<T, TDeferd1, TDeferd2> : StructuredDeferredTask<T, TDeferd1>
{
    internal Task<TDeferd2> deferredTask2;

    internal StructuredDeferredTask(Task<T> task, Task<TDeferd1> deferredTask1, Task<TDeferd2> deferredTask2)
        : this(task, deferredTask1, deferredTask2, new CancellationTokenSource()) { }

    internal StructuredDeferredTask(Task<T> task, Task<TDeferd1> deferredTask1, Task<TDeferd2> deferredTask2, CancellationTokenSource cancellationTokenSource)
        : base(task, deferredTask1, cancellationTokenSource) { this.deferredTask2 = deferredTask2; }
}

/// <summary>Represents a builder for asynchronous methods that returns a <see cref="StructuredTask{T}"/>.</summary>
/// <typeparam name="T">The type of the result.</typeparam>
public struct PoolingAsyncStructuredTaskMethodBuilder<T>
{
    // This is a placeholder for where the actual pooling logic would go.
    // In a real implementation, you would likely have a pool of reusable
    // StructuredTask<T> instances and manage them here.

    private AsyncTaskMethodBuilder<T> _builder; // Using Task builder for underlying operations

    /// <summary>Creates an instance of the <see cref="PoolingAsyncStructuredTaskMethodBuilder{T}"/> struct.</summary>
    /// <returns>The initialized instance.</returns>
    public static PoolingAsyncStructuredTaskMethodBuilder<T> Create() => default;

    /// <summary>Begins running the builder with the associated state machine.</summary>
    /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
    /// <param name="stateMachine">The state machine instance, passed by reference.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
        _builder.Start(ref stateMachine);

    /// <summary>Associates the builder with the specified state machine.</summary>
    /// <param name="stateMachine">The state machine instance to associate with the builder.</param>
    public void SetStateMachine(IAsyncStateMachine stateMachine) =>
        _builder.SetStateMachine(stateMachine);

    /// <summary>Marks the task as successfully completed.</summary>
    /// <param name="result">The result to use to complete the task.</param>
    public void SetResult(T result) =>
        _builder.SetResult(result);

    /// <summary>Marks the task as failed and binds the specified exception to the task.</summary>
    /// <param name="exception">The exception to bind to the task.</param>
    public void SetException(Exception exception) =>
        _builder.SetException(exception);

    /// <summary>Schedules the state machine to proceed to the next action when the specified awaiter completes.</summary>
    /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
    /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
    /// <param name="awaiter">the awaiter</param>
    /// <param name="stateMachine">The state machine.</param>
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        _builder.AwaitOnCompleted(ref awaiter, ref stateMachine);

    /// <summary>Schedules the state machine to proceed to the next action when the specified awaiter completes.</summary>
    /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
    /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
    /// <param name="awaiter">the awaiter</param>
    /// <param name="stateMachine">The state machine.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine =>
        _builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);

    /// <summary>Gets the task for this builder.</summary>
    public StructuredTask<T> Task => new StructuredTask<T>(_builder.Task);
}
