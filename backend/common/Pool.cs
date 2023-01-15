using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace common;

public class Pool<T>
{
    readonly ILogger _logger;
    readonly ConcurrentStack<PoolItem<T>> _available = new();
    readonly object _locker = new();
    readonly TimeSpan _waitAvailableTimeout;

    public Pool(IEnumerable<T> items, TimeSpan waitTimeout, ILogger logger) : this(items, waitTimeout)
    {
        _logger = logger;
    }

    public Pool(IEnumerable<T> items) : this(items, TimeSpan.FromSeconds(30))
    {
    }

    public Pool(IEnumerable<T> items, TimeSpan waitTimeout)
    {
        _waitAvailableTimeout = waitTimeout;
        var index = 0;
        var poolItems = items.Select(x => new PoolItem<T>(x, index++, item =>
        {
            _logger.LogInformation("Returning item at {index}", item.Index);
            _available.Push(item);
        })).Reverse();
        _available.PushRange(poolItems.ToArray());
    }

    public PoolItem<T> GetAvailableOrWait()
    {
        lock (_locker)
        {
            if (_available.TryPop(out var available))
            {
                _logger.LogInformation("Retrieving item at {index}", available.Index);
                return available;
            }

            SpinWait.SpinUntil(() => !_available.IsEmpty, _waitAvailableTimeout);
            if (_available.TryPop(out available))
            {
                _logger.LogInformation("Retrieving item at {index}", available.Index);
                return available;
            }

            throw new Exception("Pool item was not returned until timeout expired!");
        }
    }
}
