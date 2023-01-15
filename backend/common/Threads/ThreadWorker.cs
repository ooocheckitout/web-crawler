using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace common.Threads;

public class ThreadWorker : IDisposable
{
    readonly ILogger _logger;
    readonly CancellationTokenSource _cts;
    readonly Thread _thread;
    readonly ConcurrentQueue<(TaskCompletionSource, Action)> _queue = new();

    public ThreadWorker(ILogger logger)
    {
        _logger = logger;
        _cts = new CancellationTokenSource();
        _thread = new Thread(InternalLoop);
        _thread.Start();
    }

    public Task ExecuteAsync(Action action)
    {
        var tcs = new TaskCompletionSource();
        _queue.Enqueue((tcs, action));
        return tcs.Task;
    }

    void InternalLoop()
    {
        _logger.LogDebug("Worker started");
        while (!_cts.IsCancellationRequested)
        {
            Thread.Sleep(25);
            if (_queue.IsEmpty || !_queue.TryDequeue(out var actionItem)) continue;

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
        _thread.Join();
    }
}
