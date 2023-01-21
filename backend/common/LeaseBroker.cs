using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace common;

public sealed class LeaseBroker<T> : IDisposable
{
    readonly ConcurrentStack<Lease> _available = new();
    readonly ILogger<LeaseBroker<T>> _logger;

    public LeaseBroker(Func<T> factory, int numberOfItems, ILogger<LeaseBroker<T>> logger)
    {
        var internals = Enumerable
            .Range(0, numberOfItems)
            .Select(_ => new Lease {Value = factory(), ReturnLeaseAction = ReturnLease})
            .ToArray();

        _available.PushRange(internals);
        _logger = logger;
    }

    public Lease TakeLease()
    {
        if (_available.TryPop(out var available))
        {
            _logger.LogTrace("Taking available lease");
            return available;
        }

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        while (!cts.IsCancellationRequested)
        {
            if (_available.TryPop(out available))
            {
                _logger.LogTrace("Taking returned lease");
                return available;
            }
        }

        throw new Exception("Lease was not returned until timeout expired!");
    }

    public void ReturnLease(Lease lease)
    {
        _logger.LogTrace("Returning lease");
        _available.Push(lease);
    }

    public void Dispose()
    {
        foreach (var item in _available)
        {
            if (item.Value is IDisposable asDisposable) asDisposable.Dispose();
        }
    }

    public sealed class Lease : IDisposable
    {
        public T Value { get; init; }
        public Action<Lease> ReturnLeaseAction { get; init; }

        public static implicit operator T(Lease item) => item.Value;

        public void Dispose() => ReturnLeaseAction(this);
    }
}
