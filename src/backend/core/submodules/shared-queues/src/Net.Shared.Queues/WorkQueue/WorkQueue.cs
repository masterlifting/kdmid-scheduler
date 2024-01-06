using System.Collections.Concurrent;

using Net.Shared.Queues.Abstractions.Interfaces.Core.WorkQueue;

namespace Net.Shared.Queues.WorkQueue;
/// <summary>
///  Asynchronously processing functions in the concurrent queue
/// </summary>
/// <remarks>Disposable</remarks>
public sealed class WorkQueue : IWorkQueue
{
    private record WorkQueueItem(Func<Task> Func, TaskCompletionSource TaskCompletionSource);
    private readonly BlockingCollection<WorkQueueItem> _queueItems;

    public WorkQueue()
    {
        _queueItems = new();
        Task.Run(ProcessQueueItems);
    }
    public WorkQueue(int itemsCount)
    {
        _queueItems = new(itemsCount);
        Task.Run(ProcessQueueItems);
    }

    public Task Process(Func<Task> func, CancellationToken cToken)
    {
        TaskCompletionSource tcs = new(cToken);

        return _queueItems.TryAdd(new(func, tcs))
            ? tcs.Task
            : Task.CompletedTask;
    }
    public Task Process(Func<Task>[] funcs, CancellationToken cToken)
    {
        List<Task> results = new(funcs.Length);

        for (int i = 0; i < funcs.Length; i++)
        {
            TaskCompletionSource tcs = new(cToken);

            if (_queueItems.TryAdd(new(funcs[i], tcs)))
                results.Add(tcs.Task);
        }

        return Task.WhenAll(results);
    }

    public void Dispose()
    {
        _queueItems.CompleteAdding();
        _queueItems.Dispose();
    }

    private async Task ProcessQueueItems()
    {
        foreach (var item in _queueItems.GetConsumingEnumerable())
        {
            try
            {
                await item.Func.Invoke();
                item.TaskCompletionSource.SetResult();
            }
            catch (Exception exception)
            {
                item.TaskCompletionSource.SetException(exception);
            }
        }
    }
}
