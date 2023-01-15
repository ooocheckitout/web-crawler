using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace common;

public class MultiThreadWorker : IDisposable
{
    readonly ILogger _logger;
    readonly CancellationTokenSource _cts;
    readonly IEnumerable<Thread> _threads;
    readonly ConcurrentQueue<(TaskCompletionSource, Action)> _queue = new();

    public MultiThreadWorker(int numberOfThreads, ILogger logger)
    {
        _logger = logger;
        _cts = new CancellationTokenSource();
        _threads = Enumerable.Range(0, numberOfThreads).Select(_ => new Thread(InternalLoop));
        _threads.ForEach(x => x.Start());
    }

    public Task ExecuteAsync(Action action)
    {
        var tcs = new TaskCompletionSource();
        _queue.Enqueue((tcs, action));
        return tcs.Task;
    }

    public Task ExecuteManyAsync(params Action[] actions)
    {
        var tasks = new List<Task>();
        foreach (var action in actions)
        {
            var tcs = new TaskCompletionSource();
            _queue.Enqueue((tcs, action));
            tasks.Add(tcs.Task);
        }

        return Task.WhenAll(tasks);
    }

    void InternalLoop()
    {
        _logger.LogDebug("Worker started");

        Thread.Sleep(100);

        while (!_cts.IsCancellationRequested)
        {
            if (!_queue.TryDequeue(out var actionItem)) continue;

            _logger.LogDebug("Executing action");
            actionItem.Item2();
            actionItem.Item1.SetResult();
        }

        _logger.LogDebug("Worker finished");
    }

    public void Dispose()
    {
        _cts.Cancel();
        _logger.LogDebug("Worker cancelled");
        _threads.ForEach(x =>
        {
            if (x.ThreadState == ThreadState.Running) x.Join();
        });
    }
}
