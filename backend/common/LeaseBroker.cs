using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace common;

public class LeaseBroker<T> : IDisposable
{
    readonly ConcurrentStack<Lease> _available = new();
    readonly ILogger _logger;

    public LeaseBroker(Func<T> factory, int numberOfElements, ILogger logger)
    {
        var internals = Enumerable
            .Range(0, numberOfElements)
            .Select(_ => new Lease {Value = factory()})
            .ToArray();

        _available.PushRange(internals);
        _logger = logger;
    }

    public Lease TakeLease()
    {
        if (_available.TryPop(out var available))
        {
            _logger.LogDebug("Taking available lease");
            return available;
        }

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        while (!cts.IsCancellationRequested)
        {
            if (_available.TryPop(out available))
            {
                _logger.LogDebug("Taking returned lease");
                return available;
            }
        }

        throw new Exception("Lease was not returned until timeout expired!");
    }

    public void ReturnLease(Lease lease)
    {
        _logger.LogDebug("Returning lease");
        _available.Push(lease);
    }

    public void Dispose()
    {
        foreach (var item in _available)
        {
            if (item.Value is IDisposable asDisposable) asDisposable.Dispose();
        }
    }

    public class Lease
    {
        public T Value { get; init; }
    }
}
