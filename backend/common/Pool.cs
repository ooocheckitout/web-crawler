using System.Collections.Concurrent;

namespace common;

public class Pool<T>
{
    readonly ConcurrentStack<PoolItem<T>> _available = new();
    readonly object _locker = new();
    readonly TimeSpan _waitAvailableTimeout;

    public Pool(IEnumerable<T> items) : this(items, TimeSpan.FromSeconds(30))
    {
    }

    public Pool(IEnumerable<T> items, TimeSpan waitTimeout)
    {
        _waitAvailableTimeout = waitTimeout;
        var poolItems = items.Select(x => new PoolItem<T>(x, item => _available.Push(item))).Reverse();
        _available.PushRange(poolItems.ToArray());
    }

    public PoolItem<T> GetAvailableOrWait()
    {
        lock (_locker)
        {
            if (_available.TryPop(out var available))
                return available;

            SpinWait.SpinUntil(() => !_available.IsEmpty, _waitAvailableTimeout);
            if (_available.TryPop(out available))
                return available!;

            throw new Exception("Pool item was not returned until timeout expired!");
        }
    }
}
