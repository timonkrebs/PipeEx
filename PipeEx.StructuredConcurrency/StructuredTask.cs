using System.Runtime.CompilerServices;

namespace PipeEx.StructuredConcurrency;

public class StructuredTask<T> : Task<T>, IDisposable
{
    private bool mustHandleDisposing = false;

    public StructuredTask(Task<T> task, CancellationToken cancellationToken)
    : this(task, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken), false) { }

    internal StructuredTask(Task<T> task) : this(task, new CancellationTokenSource()) { }

    internal StructuredTask(StructuredTask<T> task) : this(task.Task, task.CancellationTokenSource) { }

    internal StructuredTask(Task<T> task, CancellationTokenSource cancellationTokenSource, bool mustHandleDisposing = true)
        : base(() => task.Result, cancellationTokenSource.Token)
    {
        Task = task;
        CancellationTokenSource = cancellationTokenSource;
        this.mustHandleDisposing = mustHandleDisposing;
    }

    internal Task<T> Task { get; }

    public CancellationTokenSource CancellationTokenSource { get; internal set; }

    public new TaskAwaiter<T> GetAwaiter() => Task.GetAwaiter();

    
    public new void Dispose()
    {
        base.Dispose();
        if (mustHandleDisposing) CancellationTokenSource.Dispose();
    }
}

public class StructuredDeferedTask<T, TDeferd> : StructuredTask<T>
{
    internal Task<TDeferd> deferedTask1;

    internal StructuredDeferedTask(Task<T> task, Task<TDeferd> deferedTask)
        : this(task, deferedTask, new CancellationTokenSource()) { }

    internal StructuredDeferedTask(Task<T> task, Task<TDeferd> deferedTask1, CancellationTokenSource cancellationTokenSource)
        : base(task, cancellationTokenSource) 
        { 
            this.deferedTask1 = deferedTask1;
        }
}

public class StructuredDeferedTask<T, TDeferd1, TDeferd2> : StructuredDeferedTask<T, TDeferd1>
{
    internal Task<TDeferd2> deferedTask2;

    internal StructuredDeferedTask(Task<T> task, Task<TDeferd1> deferedTask1, Task<TDeferd2> deferedTask2)
        : this(task, deferedTask1, deferedTask2, new CancellationTokenSource()) { }

    internal StructuredDeferedTask(Task<T> task, Task<TDeferd1> deferedTask1, Task<TDeferd2> deferedTask2, CancellationTokenSource cancellationTokenSource)
        : base(task, deferedTask1, cancellationTokenSource) 
        { 
            this.deferedTask2 = deferedTask2;
        }
}