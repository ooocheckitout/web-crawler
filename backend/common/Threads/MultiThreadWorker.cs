using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace common.Threads;

public sealed class MultiThreadWorker : IDisposable
{
    readonly ILogger<MultiThreadWorker> _logger;
    readonly CancellationTokenSource _cts;
    readonly IEnumerable<Thread> _threads;
    readonly ConcurrentQueue<(TaskCompletionSource TaskCompletionSource, Func<Task> ActionAsync)> _queue = new();

    public MultiThreadWorker(int numberOfThreads, ILogger<MultiThreadWorker> logger)
    {
        _logger = logger;
        _cts = new CancellationTokenSource();
        _threads = Enumerable.Range(0, numberOfThreads).Select(index => new Thread(InternalLoop) { Name = $"{nameof(MultiThreadWorker)} {index}" });
        _threads.ForEach(x => x.Start());
        _logger.LogInformation("Initialized {numberOfThreads} threads", numberOfThreads);
    }

    Task ExecuteAsync(Func<Task> action)
    {
        var tcs = new TaskCompletionSource();
        _queue.Enqueue((tcs, action));
        return tcs.Task;
    }

    public Task ExecuteManyAsync(params Func<Task>[] actions)
    {
        return Task.WhenAll(actions.Select(ExecuteAsync));
    }

    void InternalLoop()
    {
        _logger.LogTrace("Worker started");

        Thread.Sleep(100);

        while (!_cts.IsCancellationRequested)
        {
            if (!_queue.TryDequeue(out var actionItem))
            {
                _logger.LogTrace("No items in the queue. {threadName} skipping...", Thread.CurrentThread.Name);
                Thread.Sleep(100);
                continue;
            }

            Task.Run(async () => await ExecuteItemAsync(actionItem)).Wait();
        }

        _logger.LogTrace("Worker finished");
    }

    async Task ExecuteItemAsync((TaskCompletionSource TaskCompletionSource, Func<Task> ActionAsync) actionItem)
    {
        _logger.LogTrace("Start action execution");

        try
        {
            await actionItem.ActionAsync();
            actionItem.TaskCompletionSource.SetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured during action execution");
            actionItem.TaskCompletionSource.SetException(ex);
        }

        _logger.LogTrace("Finish action execution");
    }

    public void Dispose()
    {
        _cts.Cancel();
        _logger.LogTrace("Worker cancelled");
        _threads.ForEach(x =>
        {
            if (x.ThreadState == ThreadState.Running) x.Join();
        });
    }
}
