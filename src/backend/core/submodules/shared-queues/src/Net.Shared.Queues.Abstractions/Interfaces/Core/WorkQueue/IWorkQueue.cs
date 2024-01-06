namespace Net.Shared.Queues.Abstractions.Interfaces.Core.WorkQueue;

/// <summary>
///  Asynchronously processing functions in the concurrent queue
/// </summary>
/// <remarks>Disposable</remarks>
public interface IWorkQueue : IDisposable
{
    Task Process(Func<Task> func, CancellationToken cToken);
    Task Process(Func<Task>[] funcs, CancellationToken cToken);
}
