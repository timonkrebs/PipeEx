using System.Runtime.CompilerServices;

namespace PipeEx.StructuredConcurrency;

[AsyncMethodBuilder(typeof(PoolingAsyncStructuredTaskMethodBuilder<>))]
public class StructuredTask<T>
{
    private Task<T> task;
    public CancellationTokenSource CancellationTokenSource { get; }

    public StructuredTask(Task<T> task, CancellationToken cancellationToken)
    {
        Task = task;
        CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    }
    
    internal StructuredTask(Task<T> task)
    {
        Task = task;
        CancellationTokenSource = new CancellationTokenSource();
    }

    internal StructuredTask(Task<T> task, CancellationTokenSource cancellationTokenSource)
    {
        Task = task;
        CancellationTokenSource = cancellationTokenSource;
    }

    internal StructuredTask(StructuredTask<T> task)
    {
        Task = task.Task;
        CancellationTokenSource = task.CancellationTokenSource;
    }

    public TaskAwaiter<T> GetAwaiter() => Task.GetAwaiter();

    public static implicit operator Task<T>(StructuredTask<T> structuredTask)
    {
        return structuredTask.Task;
    }
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
